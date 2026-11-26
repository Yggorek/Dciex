
using Dciex.Application.Configuration;
using Dciex.Core.Configuration;
using Dciex.Infrastructure.Configuration;
using YamlDotNet.Core;

public class ConfigurationLoaderTests
{
    
    [Fact]
    public async Task LoadAsync_ValidConfiguration_ReturnsValidResult()
    {
        // Arrange
        var conf = new EnvironmentConfiguration
        {
            Name = "TestEnvironment",
            EnvironmentMode = EnvironmentMode.External
        };
        

        var reader = new FakeConfigurationReader(conf);
        var validator = new ConfigurationValidator();
        var loader = new ConfigurationLoader(reader, validator);


        // Act
         var result = await loader.LoadAsync("fake-path.yaml");

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.Configuration);
        Assert.Empty(result.LoadErrors);
        Assert.Same(conf,result.Configuration);
    }

    [Fact]
    public async Task LoadAsync_EmptyYaml_ReturnsInvalidConfigurationDataError()
    {
        // Arrange
        const string yaml = "";

        // Act
        var result = await LoadFromYamlAsync(yaml);

        // Assert
        Assert.False(result.IsValid);
        var error = Assert.Single(result.LoadErrors);
        Assert.Equal(ConfigurationLoadErrorCode.InvalidConfigurationData, error.Code);
        Assert.Equal("Configuration file is empty or contains no configuration data.", error.Message);
    }

    [Fact]
    public async Task LoadAsync_InvalidYaml_ReturnsInvalidYamlError()
    {
        // Arrange
        const string yaml = "name: [unclosed";

        // Act
        var result = await LoadFromYamlAsync(yaml);

        // Assert
        Assert.False(result.IsValid);
        var error = Assert.Single(result.LoadErrors);
        Assert.Equal(ConfigurationLoadErrorCode.InvalidYaml, error.Code);
        Assert.Equal("Configuration file contains invalid YAML.", error.Message);
    }

    [Fact]
    public async Task YamlConfigurationReader_InvalidYaml_ThrowsConfigurationReadException()
    {
        // Arrange
        var path = await WriteTempYamlAsync("name: [unclosed");
        try
        {
            var reader = new YamlConfigurationReader();

            // Act
            var exception = await Assert.ThrowsAsync<ConfigurationReadException>(
                () => reader.LoadAsync(path));

            // Assert
            Assert.IsType<YamlException>(exception.InnerException);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static async Task<ConfigurationLoadResult> LoadFromYamlAsync(string yaml)
    {
        var path = await WriteTempYamlAsync(yaml);
        try
        {
            var loader = new ConfigurationLoader(
                new YamlConfigurationReader(),
                new ConfigurationValidator());

            return await loader.LoadAsync(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static async Task<string> WriteTempYamlAsync(string yaml)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dciex-{Guid.NewGuid():N}.yaml");
        await File.WriteAllTextAsync(path, yaml);
        return path;
    }

    private class FakeConfigurationReader : IConfigurationReader
    {
        private readonly EnvironmentConfiguration _configuration;

        public FakeConfigurationReader(EnvironmentConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<EnvironmentConfiguration> LoadAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_configuration);
        }
    }
    [Fact]
    public async Task LoadAsync_ExistingComposeSource_ReturnsExisting()
    {
        // Arrange
        var yaml = """
        name: musicplayer
        environmentMode: external
        compose:
          source: existing
        """;

        // Act
        var result = await LoadFromYamlAsync(yaml);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.Configuration);
        Assert.NotNull(result.Configuration.Compose);
        Assert.Equal(ComposeSource.Existing, result.Configuration.Compose.Source);
    }
    [Fact]
    public async Task LoadAsync_GeneratedComposeSource_ReturnsGenerated()
    {
        // Arrange
        var yaml = """
        name: musicplayer
        environmentMode: external
        compose:
          source: generated
        """;

        // Act
        var result = await LoadFromYamlAsync(yaml);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.Configuration);
        Assert.NotNull(result.Configuration.Compose);
        Assert.Equal(ComposeSource.Generated, result.Configuration.Compose.Source);
    }
    [Fact]
    public async Task LoadAsync_WithoutComposeSection_ComposeIsNull()
    {
        // Arrange
        var yaml = """
        name: musicplayer
        environmentMode: external
        """;

        // Act
        var result = await LoadFromYamlAsync(yaml);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.Configuration);
        Assert.Null(result.Configuration.Compose);
    }
    [Fact]
    public async Task LoadAsync_UnknownComposeSource_ReturnsInvalidYamlError()
    {
        // Arrange
        var yaml = """
        name: musicplayer
        environmentMode: external
        compose:
          source: unknow
        """;

        // Act
        var result = await LoadFromYamlAsync(yaml);

        // Assert
        Assert.False(result.IsValid);

        var error = Assert.Single(result.LoadErrors);
        Assert.Equal(ConfigurationLoadErrorCode.InvalidYaml, error.Code);
    }
}
