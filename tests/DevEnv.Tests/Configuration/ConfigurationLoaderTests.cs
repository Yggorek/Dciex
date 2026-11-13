
using DevEnv.Application.Configuration;
using DevEnv.Core.Configuration;

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
}
