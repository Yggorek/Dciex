using Dciex.Application.Configuration;
using Dciex.Application.Projects;
using Dciex.Application.Environments.Abstractions;
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

        public EnvironmentOrchestrator(IProjectRegistry projectRegistry,ConfigurationLoader configurationLoader,
                                       IComposeFileGenerator composeFileGenerator, IFileManager fileManager,
                                       IDockerComposeManager dockerComposeManager)
        {
             ArgumentNullException.ThrowIfNull(projectRegistry);
             ArgumentNullException.ThrowIfNull(configurationLoader);
             ArgumentNullException.ThrowIfNull(composeFileGenerator);
             ArgumentNullException.ThrowIfNull(fileManager);
             ArgumentNullException.ThrowIfNull(dockerComposeManager);
            _projectRegistry = projectRegistry;
            _configurationLoader = configurationLoader;
            _composeFileGenerator = composeFileGenerator;
            _fileManager = fileManager;
            _dockerComposeManager = dockerComposeManager;
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
            string composePath = Path.Combine(project.Path, "compose.yaml");
            var loadResult = await _configurationLoader.LoadAsync(configPath, cancellationToken);
            if (!loadResult.IsValid)
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
            var configuration = loadResult.Configuration!;
            string composeYaml;
            try
            {
               composeYaml = _composeFileGenerator.Generate(configuration);
            }
            catch (ArgumentException ex)
            {
                var message = $"Failed to generate Docker Compose configuration: {ex.Message}";
                return new EnvironmentOperationResult
                {
                    IsSuccess = false,
                    Message = message,
                    ErrorCode = EnvironmentErrorCode.ComposeGenerationFailed,
                    Errors =
                    [
                        new EnvironmentOperationError
                        {
                            Code = EnvironmentErrorCode.ComposeGenerationFailed.ToString(),
                            Message = message,
                            Source = EnvironmentOperationErrorSource.ComposeFileGenerator
                        }
                    ]
                };
            }
            catch (InvalidOperationException ex)
            {
                var message = $"Docker Compose generation failed: {ex.Message}";
                return new EnvironmentOperationResult
                {
                    IsSuccess = false,
                    Message = message,
                    ErrorCode = EnvironmentErrorCode.ComposeGenerationFailed,
                    Errors =
                    [
                        new EnvironmentOperationError
                        {
                            Code = EnvironmentErrorCode.ComposeGenerationFailed.ToString(),
                            Message = message,
                            Source = EnvironmentOperationErrorSource.ComposeFileGenerator
                        }
                    ]
                };
            }
            try
            {
                await _fileManager.WriteAllTextAsync(composePath, composeYaml, cancellationToken);
            }
            catch (IOException ex)
            {
                var message = $"Failed to write Docker Compose file: {ex.Message}";
                return new EnvironmentOperationResult
                {
                  IsSuccess = false,
                  Message = message,
                  ErrorCode = EnvironmentErrorCode.FileWriteFailed,
                  Errors =
                  [
                      new EnvironmentOperationError
                      {
                          Code = EnvironmentErrorCode.FileWriteFailed.ToString(),
                          Message = message,
                          Source = EnvironmentOperationErrorSource.FileManager
                      }
                  ]
                };
            }
            catch(UnauthorizedAccessException ex)
            {
                var message = $"Failed to write Docker Compose file: {ex.Message}";
                return new EnvironmentOperationResult
                {
                    IsSuccess = false,
                    Message = message,
                    ErrorCode = EnvironmentErrorCode.FileWriteFailed,
                    Errors =
                    [
                        new EnvironmentOperationError
                        {
                            Code = EnvironmentErrorCode.FileWriteFailed.ToString(),
                            Message = message,
                            Source = EnvironmentOperationErrorSource.FileManager
                        }
                    ]
                };
            }
            try
            {
                await _dockerComposeManager.StartAsync(
                 project.Path,
                 cancellationToken);
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
        }
    }
}
