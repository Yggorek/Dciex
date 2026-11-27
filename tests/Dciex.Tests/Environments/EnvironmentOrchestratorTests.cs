using Dciex.Application.Compose;
using Dciex.Application.Configuration;
using Dciex.Application.Environments;
using Dciex.Application.Environments.Abstractions;
using Dciex.Application.Projects;
using Dciex.Core.Configuration;
using Dciex.Core.Projects;

namespace Dciex.Tests.Environments;

public sealed class EnvironmentOrchestratorTests
{
    private const string ProjectPath = "C:/projects/musicplayer";
    private const string ExistingComposePath = "C:/projects/musicplayer/docker-compose.yml";

    [Fact]
    public async Task UpAsync_GeneratedConfiguration_GeneratesWritesAndStartsOnce()
    {
        // Arrange
        var rig = new OrchestratorRig(ValidConfiguration());

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, rig.Generator.Calls);
        Assert.Equal(1, rig.Files.Calls);
        Assert.Equal(0, rig.Finder.Calls);
        Assert.Equal(1, rig.Docker.Calls);
        Assert.Equal(Path.Combine(ProjectPath, ".dciex", "compose.yaml"), rig.Docker.ComposePath);
    }

    [Fact]
    public async Task UpAsync_ConfigurationWithoutComposeSection_UsesGeneratedSource()
    {
        // Arrange
        var configuration = ValidConfiguration();
        configuration.Compose = null;
        var rig = new OrchestratorRig(configuration);

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, rig.Generator.Calls);
        Assert.Equal(1, rig.Files.Calls);
        Assert.Equal(0, rig.Finder.Calls);
        Assert.Equal(1, rig.Docker.Calls);
    }

    [Fact]
    public async Task UpAsync_ExistingConfiguration_UsesFoundFileWithoutGenerationOrWrite()
    {
        // Arrange
        var rig = new OrchestratorRig(ValidConfiguration(ComposeSource.Existing));
        rig.Finder.Result = Found(ExistingComposePath);

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, rig.Generator.Calls);
        Assert.Equal(0, rig.Files.Calls);
        Assert.Equal(1, rig.Finder.Calls);
        Assert.Equal(1, rig.Docker.Calls);
        Assert.Equal(ExistingComposePath, rig.Docker.ComposePath);
    }

    [Fact]
    public async Task UpAsync_MissingConfigurationAndFoundCompose_StartsExistingFile()
    {
        // Arrange
        var rig = new OrchestratorRig(new FileNotFoundException());
        rig.Finder.Result = Found(ExistingComposePath);

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, rig.Generator.Calls);
        Assert.Equal(0, rig.Files.Calls);
        Assert.Equal(1, rig.Finder.Calls);
        Assert.Equal(1, rig.Docker.Calls);
        Assert.Equal(ExistingComposePath, rig.Docker.ComposePath);
    }

    [Fact]
    public async Task UpAsync_MissingConfigurationAndCompose_ReturnsComposeNotFound()
    {
        // Arrange
        var rig = new OrchestratorRig(new FileNotFoundException());
        rig.Finder.Result = new ComposeDiscoveryResult { Status = ComposeDiscoveryStatus.NotFound };

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EnvironmentErrorCode.ComposeNotFound, result.ErrorCode);
        Assert.Equal(0, rig.Docker.Calls);
    }

    [Fact]
    public async Task UpAsync_ComposeFinderIOException_ReturnsComposeDiscoveryFailed()
    {
        // Arrange
        var rig = new OrchestratorRig(ValidConfiguration(ComposeSource.Existing));
        rig.Finder.Failure = new IOException("search failed");

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EnvironmentErrorCode.ComposeDiscoveryFailed, result.ErrorCode);
        Assert.Equal(1, rig.Finder.Calls);
        Assert.Equal(0, rig.Docker.Calls);
    }

    [Fact]
    public async Task UpAsync_ComposeFinderUnauthorizedAccess_ReturnsComposeDiscoveryFailed()
    {
        // Arrange
        var rig = new OrchestratorRig(ValidConfiguration(ComposeSource.Existing));
        rig.Finder.Failure = new UnauthorizedAccessException("denied");

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EnvironmentErrorCode.ComposeDiscoveryFailed, result.ErrorCode);
        Assert.Equal(1, rig.Finder.Calls);
        Assert.Equal(0, rig.Docker.Calls);
    }

    [Fact]
    public async Task UpAsync_InvalidComposeSource_FailsValidationWithoutSearchingOrStarting()
    {
        // Arrange
        var rig = new OrchestratorRig(ValidConfiguration((ComposeSource)int.MaxValue));

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EnvironmentErrorCode.ConfigurationLoadFailed, result.ErrorCode);
        Assert.Contains(result.Errors, error => error.Code == "ComposeSourceInvalid");
        Assert.Equal(0, rig.Finder.Calls);
        Assert.Equal(0, rig.Docker.Calls);
    }

    [Fact]
    public async Task UpAsync_MultipleExistingComposeFiles_ReturnsComposeAmbiguousAndListsFiles()
    {
        // Arrange
        var rig = new OrchestratorRig(ValidConfiguration(ComposeSource.Existing));
        rig.Finder.Result = new ComposeDiscoveryResult
        {
            Status = ComposeDiscoveryStatus.Ambiguous,
            Paths = ["compose.yaml", "docker-compose.yml"]
        };

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EnvironmentErrorCode.ComposeAmbiguous, result.ErrorCode);
        Assert.Contains("compose.yaml", result.Message);
        Assert.Contains("docker-compose.yml", result.Message);
        Assert.Equal(0, rig.Docker.Calls);
    }

    [Fact]
    public async Task UpAsync_MissingProjectDirectory_ReturnsProjectDirectoryNotFound()
    {
        // Arrange
        var rig = new OrchestratorRig(ValidConfiguration(ComposeSource.Existing));
        rig.Finder.Result = new ComposeDiscoveryResult { Status = ComposeDiscoveryStatus.DirectoryNotFound };

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EnvironmentErrorCode.ProjectDirectoryNotFound, result.ErrorCode);
        Assert.Equal(0, rig.Docker.Calls);
    }

    [Fact]
    public async Task UpAsync_InvalidYaml_DoesNotSearchOrStartCompose()
    {
        // Arrange
        var rig = new OrchestratorRig(new ConfigurationReadException("bad yaml", new FormatException()));

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EnvironmentErrorCode.ConfigurationLoadFailed, result.ErrorCode);
        Assert.Equal(0, rig.Finder.Calls);
        Assert.Equal(0, rig.Docker.Calls);
    }

    [Fact]
    public async Task UpAsync_InaccessibleConfiguration_DoesNotSearchOrStartCompose()
    {
        // Arrange
        var rig = new OrchestratorRig(new UnauthorizedAccessException("denied"));

        // Act
        var result = await rig.Orchestrator.UpAsync("musicplayer");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EnvironmentErrorCode.ConfigurationLoadFailed, result.ErrorCode);
        Assert.Equal(ConfigurationLoadErrorCode.AccessDenied.ToString(), Assert.Single(result.Errors).Code);
        Assert.Equal(0, rig.Finder.Calls);
        Assert.Equal(0, rig.Docker.Calls);
    }

    [Fact]
    public async Task UpAsync_CancellationDuringConfigurationLoad_Propagates()
    {
        // Arrange
        var cancellation = new OperationCanceledException();
        var rig = new OrchestratorRig(cancellation);

        // Act
        var thrown = await Assert.ThrowsAsync<OperationCanceledException>(
            () => rig.Orchestrator.UpAsync("musicplayer"));

        // Assert
        Assert.Same(cancellation, thrown);
        Assert.Equal(0, rig.Finder.Calls);
        Assert.Equal(0, rig.Docker.Calls);
    }

    private static EnvironmentConfiguration ValidConfiguration(ComposeSource source = ComposeSource.Generated) => new()
    {
        Name = "musicplayer",
        EnvironmentMode = EnvironmentMode.External,
        Compose = new ComposeSettings { Source = source },
        Services = source == ComposeSource.Generated ? [new ServiceConfiguration { Name = "web", Image = "nginx" }] : null
    };

    private static ComposeDiscoveryResult Found(string path) => new()
    {
        Status = ComposeDiscoveryStatus.Found,
        Paths = [path]
    };

    private sealed class OrchestratorRig
    {
        public TrackingGenerator Generator { get; } = new();
        public TrackingFileManager Files { get; } = new();
        public TrackingFinder Finder { get; } = new();
        public TrackingDocker Docker { get; } = new();
        public EnvironmentOrchestrator Orchestrator { get; }

        public OrchestratorRig(Exception configurationException)
            : this(new ConfigReader(exception: configurationException)) { }

        public OrchestratorRig(EnvironmentConfiguration configuration)
            : this(new ConfigReader(configuration)) { }

        private OrchestratorRig(ConfigReader reader)
        {
            var registry = new StubProjectRegistry();
            var loader = new ConfigurationLoader(reader, new ConfigurationValidator());
            Orchestrator = new EnvironmentOrchestrator(registry, loader, Generator, Files, Docker, Finder);
        }
    }

    private sealed class ConfigReader : IConfigurationReader
    {
        private readonly EnvironmentConfiguration? _configuration;
        private readonly Exception? _exception;
        public ConfigReader(EnvironmentConfiguration? configuration = null, Exception? exception = null)
        {
            _configuration = configuration;
            _exception = exception;
        }

        public Task<EnvironmentConfiguration> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            if (_exception is not null) return Task.FromException<EnvironmentConfiguration>(_exception);
            return Task.FromResult(_configuration!);
        }
    }

    private sealed class StubProjectRegistry : IProjectRegistry
    {
        public Task<Project?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
            Task.FromResult<Project?>(new Project { Name = name, Path = ProjectPath });
        public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Project> RegisterAsync(string path, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> UnregisterAsync(string name, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class TrackingGenerator : IComposeFileGenerator
    {
        public int Calls { get; private set; }
        public string Generate(EnvironmentConfiguration configuration) { Calls++; return "generated compose"; }
    }

    private sealed class TrackingFileManager : IFileManager
    {
        public int Calls { get; private set; }
        public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
        { Calls++; return Task.CompletedTask; }
        public Task WriteAllTextAtomicAsync(string path, string content, CancellationToken cancellationToken = default)
        { Calls++; return Task.CompletedTask; }
    }

    private sealed class TrackingFinder : IComposeFinder
    {
        public int Calls { get; private set; }
        public ComposeDiscoveryResult Result { get; set; } = Found(ExistingComposePath);
        public Exception? Failure { get; set; }
        public ComposeDiscoveryResult Find(string path, CancellationToken cancellationToken = default)
        {
            Calls++;
            if (Failure is not null) throw Failure;
            return Result;
        }
    }

    private sealed class TrackingDocker : IDockerComposeManager
    {
        public int Calls { get; private set; }
        public string? ComposePath { get; private set; }
        public Task<DockerComposeExecutionResult> StartAsync(string composeFilePath, string workingDirectory,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            ComposePath = composeFilePath;
            return Task.FromResult(new DockerComposeExecutionResult(0, string.Empty, string.Empty));
        }
        public Task<DockerComposeExecutionResult> StopAsync(string composeFilePath, string workingDirectory,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
