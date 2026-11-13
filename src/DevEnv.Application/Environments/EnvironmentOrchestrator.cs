using DevEnv.Application.Configuration;
using DevEnv.Application.Projects;
using DevEnv.Application.Environments.Abstractions;

namespace DevEnv.Application.Environments
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
            var project = await _projectRegistry.GetByNameAsync(projectName, cancellationToken);

            if(project == null)
            {
                return new EnvironmentOperationResult
                {
                    IsSuccess = false,
                    Message = $"Project {projectName} not found",
                    ErrorCode = EnvironmentErrorCode.ProjectNotFound
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
                    ErrorCode = EnvironmentErrorCode.ConfigurationLoadFailed
                };
            }
            var configuration = loadResult.Configuration!;
            var composeYaml = _composeFileGenerator.Generate(configuration);
            await _fileManager.WriteAllTextAsync(composePath, composeYaml, cancellationToken);
            await _dockerComposeManager.StartAsync(
                 project.Path,
                 cancellationToken);
            
            return new EnvironmentOperationResult
            {
                IsSuccess = true,
                Message = "Environment started successfully."
            };
        }
    }
}