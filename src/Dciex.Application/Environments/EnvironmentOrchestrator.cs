using Dciex.Application.Configuration;
using Dciex.Application.Projects;
using Dciex.Application.Environments.Abstractions;
using Dciex.Application.Compose;
using Dciex.Core.Configuration;
using System.Text.Json;

namespace Dciex.Application.Environments
{
    public class EnvironmentOrchestrator
    {
        private readonly IProjectRegistry _projectRegistry;
        private readonly ConfigurationLoader _configurationLoader;
        private readonly IComposeFileGenerator _composeFileGenerator;
        private readonly IFileManager _fileManager;
        private readonly IDockerComposeManager _dockerComposeManager;
        private readonly IComposeFinder _composeFinder;

        public EnvironmentOrchestrator(IProjectRegistry projectRegistry,ConfigurationLoader configurationLoader,
                                       IComposeFileGenerator composeFileGenerator, IFileManager fileManager,
                                       IDockerComposeManager dockerComposeManager,
                                       IComposeFinder composeFinder)
        {
             ArgumentNullException.ThrowIfNull(projectRegistry);
             ArgumentNullException.ThrowIfNull(configurationLoader);
             ArgumentNullException.ThrowIfNull(composeFileGenerator);
             ArgumentNullException.ThrowIfNull(fileManager);
             ArgumentNullException.ThrowIfNull(dockerComposeManager);
             ArgumentNullException.ThrowIfNull(composeFinder);
            _projectRegistry = projectRegistry;
            _configurationLoader = configurationLoader;
            _composeFileGenerator = composeFileGenerator;
            _fileManager = fileManager;
            _dockerComposeManager = dockerComposeManager;
            _composeFinder = composeFinder;
        }
        public async Task<EnvironmentOperationResult> UpAsync(string projectName, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(projectName);
            Dciex.Core.Projects.Project? project;
            try
            {
                project = await _projectRegistry.GetByNameAsync(projectName, cancellationToken);
            }
            catch (IOException ex)
            {
                return ProjectRegistryFailure($"Failed to read registered projects: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                return ProjectRegistryFailure($"Access to registered projects was denied: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return ProjectRegistryFailure($"Registered projects data is invalid: {ex.Message}");
            }
            catch (InvalidDataException ex)
            {
                return ProjectRegistryFailure($"Registered projects data is invalid: {ex.Message}");
            }

            if(project == null)
            {
                var message = $"Project {projectName} not found";
                return new EnvironmentOperationResult
                {
                    IsSuccess = false,
                    Message = message,
                    ErrorCode = EnvironmentErrorCode.ProjectNotFound,
                    Errors =
                    [
                        new EnvironmentOperationError
                        {
                            Code = EnvironmentErrorCode.ProjectNotFound.ToString(),
                            Message = message,
                            Source = EnvironmentOperationErrorSource.ProjectRegistry
                        }
                    ]
                };
            }
            
            string configPath = Path.Combine(project.Path, "devenv.yaml");
            var loadResult = await _configurationLoader.LoadAsync(configPath, cancellationToken);
            var useExistingCompose = !loadResult.IsValid &&
                                     loadResult.LoadErrors.Count == 1 &&
                                     loadResult.LoadErrors[0].Code == ConfigurationLoadErrorCode.FileNotFound &&
                                     loadResult.ValidationErrors.Count == 0;
            if (!loadResult.IsValid && !useExistingCompose)
            {
                return new EnvironmentOperationResult
                {
                    IsSuccess = false,
                    Message = $"Failed to load or validate environment configuration.",
                    ErrorCode = EnvironmentErrorCode.ConfigurationLoadFailed,
                    Errors = loadResult.LoadErrors
                        .Select(error => new EnvironmentOperationError
                        {
                            Code = error.Code.ToString(),
                            Message = error.Message,
                            Source = EnvironmentOperationErrorSource.ConfigurationLoader
                        })
                        .Concat(loadResult.ValidationErrors.Select(error => new EnvironmentOperationError
                        {
                            Code = error.Code,
                            Message = error.ErrorMessage,
                            Source = EnvironmentOperationErrorSource.ConfigurationValidator,
                            PropertyName = error.PropertyName
                        }))
                        .ToArray()
                };
            }
            string composePath;
            var composeSource = useExistingCompose
                ? ComposeSource.Existing
                : loadResult.Configuration!.Compose?.Source ?? ComposeSource.Generated;

            if (composeSource == ComposeSource.Existing)
            {
                ComposeDiscoveryResult discovery;
                try
                {
                    discovery = _composeFinder.Find(project.Path, cancellationToken);
                }
                catch (IOException ex)
                {
                    return ComposeDiscoveryFailure(EnvironmentErrorCode.ComposeDiscoveryFailed,
                        $"Failed to search for a Docker Compose file: {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    return ComposeDiscoveryFailure(EnvironmentErrorCode.ComposeDiscoveryFailed,
                        $"Access to the project directory was denied: {ex.Message}");
                }

                switch (discovery.Status)
                {
                    case ComposeDiscoveryStatus.Found when discovery.SinglePath is not null:
                        composePath = discovery.SinglePath;
                        break;
                    case ComposeDiscoveryStatus.NotFound:
                        return ComposeDiscoveryFailure(EnvironmentErrorCode.ComposeNotFound,
                            $"No Docker Compose file was found in project directory '{project.Path}'.");
                    case ComposeDiscoveryStatus.Ambiguous:
                        var paths = string.Join(Environment.NewLine, discovery.Paths);
                        return ComposeDiscoveryFailure(EnvironmentErrorCode.ComposeAmbiguous,
                            $"Multiple Docker Compose files were found in project directory '{project.Path}':{Environment.NewLine}{paths}");
                    case ComposeDiscoveryStatus.DirectoryNotFound:
                        return ComposeDiscoveryFailure(EnvironmentErrorCode.ProjectDirectoryNotFound,
                            $"Project directory '{project.Path}' was not found.");
                    default:
                        return ComposeDiscoveryFailure(EnvironmentErrorCode.ComposeDiscoveryFailed,
                            $"Compose discovery returned an unsupported status for project directory '{project.Path}'.");
                }
            }
            else
            {
                composePath = Path.Combine(project.Path, ".dciex", "compose.yaml");
                string composeYaml;
                try
                {
                   composeYaml = _composeFileGenerator.Generate(loadResult.Configuration!);
                }
                catch (ArgumentException ex)
                {
                    return ComposeGenerationFailure($"Failed to generate Docker Compose configuration: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    return ComposeGenerationFailure($"Docker Compose generation failed: {ex.Message}");
                }
                try
                {
                    await _fileManager.WriteAllTextAtomicAsync(composePath, composeYaml, cancellationToken);
                }
                catch (IOException ex)
                {
                    return FileWriteFailure(ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    return FileWriteFailure(ex.Message);
                }
            }
            try
            {
                var composeResult = await _dockerComposeManager.StartAsync(
                    composePath,
                    project.Path,
                    cancellationToken);
                if (!composeResult.IsSuccess)
                {
                    var diagnostics = string.Join(
                        Environment.NewLine,
                        new[] { composeResult.StandardError, composeResult.StandardOutput }
                            .Where(text => !string.IsNullOrWhiteSpace(text)));
                    var message = $"Docker Compose exited with code {composeResult.ExitCode}.";
                    if (!string.IsNullOrWhiteSpace(diagnostics))
                    {
                        message = $"{message}{Environment.NewLine}{diagnostics}";
                    }

                    return new EnvironmentOperationResult
                    {
                        IsSuccess = false,
                        Message = message,
                        ErrorCode = EnvironmentErrorCode.DockerStartFailed,
                        Errors =
                        [
                            new EnvironmentOperationError
                            {
                                Code = EnvironmentErrorCode.DockerStartFailed.ToString(),
                                Message = message,
                                Source = EnvironmentOperationErrorSource.DockerComposeManager
                            }
                        ]
                    };
                }
            }
            catch (InvalidOperationException ex)
            {
                var message = $"Failed to start Docker Compose: {ex.Message}";
                return new EnvironmentOperationResult
                {
                    IsSuccess = false,
                    Message = message,
                    ErrorCode = EnvironmentErrorCode.DockerStartFailed,
                    Errors =
                    [
                        new EnvironmentOperationError
                        {
                            Code = EnvironmentErrorCode.DockerStartFailed.ToString(),
                            Message = message,
                            Source = EnvironmentOperationErrorSource.DockerComposeManager
                        }
                    ]
                };
            }
            catch (IOException ex)
            {
                return DockerStartFailure(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return DockerStartFailure(ex.Message);
            }
            return new EnvironmentOperationResult
            {
                IsSuccess = true,
                Message = "Environment started successfully."
            };

            EnvironmentOperationResult ProjectRegistryFailure(string message) => new()
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = EnvironmentErrorCode.ProjectRegistryLoadFailed,
                Errors =
                [
                    new EnvironmentOperationError
                    {
                        Code = EnvironmentErrorCode.ProjectRegistryLoadFailed.ToString(),
                        Message = message,
                        Source = EnvironmentOperationErrorSource.ProjectRegistry
                    }
                ]
            };

            EnvironmentOperationResult ComposeDiscoveryFailure(EnvironmentErrorCode code, string message) => new()
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = code,
                Errors = [new EnvironmentOperationError { Code = code.ToString(), Message = message,
                    Source = EnvironmentOperationErrorSource.ComposeFinder }]
            };

            EnvironmentOperationResult ComposeGenerationFailure(string message) => new()
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = EnvironmentErrorCode.ComposeGenerationFailed,
                Errors = [new EnvironmentOperationError { Code = EnvironmentErrorCode.ComposeGenerationFailed.ToString(),
                    Message = message, Source = EnvironmentOperationErrorSource.ComposeFileGenerator }]
            };

            EnvironmentOperationResult FileWriteFailure(string detail)
            {
                var message = $"Failed to write Docker Compose file: {detail}";
                return new EnvironmentOperationResult
                {
                    IsSuccess = false,
                    Message = message,
                    ErrorCode = EnvironmentErrorCode.FileWriteFailed,
                    Errors = [new EnvironmentOperationError { Code = EnvironmentErrorCode.FileWriteFailed.ToString(),
                        Message = message, Source = EnvironmentOperationErrorSource.FileManager }]
                };
            }

            EnvironmentOperationResult DockerStartFailure(string detail)
            {
                var message = $"Failed to start Docker Compose: {detail}";
                return new EnvironmentOperationResult
                {
                    IsSuccess = false,
                    Message = message,
                    ErrorCode = EnvironmentErrorCode.DockerStartFailed,
                    Errors = [new EnvironmentOperationError { Code = EnvironmentErrorCode.DockerStartFailed.ToString(),
                        Message = message, Source = EnvironmentOperationErrorSource.DockerComposeManager }]
                };
            }
        }
    }
}
