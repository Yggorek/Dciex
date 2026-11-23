using Dciex.Core.Configuration;
using Dciex.Infrastructure.Docker.Compose;
using System.Collections.Generic;
using Xunit;

public class ComposeGeneratorTests
{
    [Fact]
    public void Generate_WithTwoServices_ReturnsValidComposeConfiguration()
    {
        // Arrange
        var config = new EnvironmentConfiguration
        {
            Services = new List<ServiceConfiguration>
            {
                new ServiceConfiguration
                {
                    Name = "postgres",
                    Image = "postgres:17",
                    Port = 5432
                },
                new ServiceConfiguration
                {
                    Name = "web",
                    Image = "nginx:latest",
                    Port = 80
                }
            }
        };

        // Act
        var generator = new ComposeGenerator();
        var result = generator.Generate(config);

        // Assert
        Assert.NotNull(result.Services);
        Assert.Equal(2, result.Services.Count);
        Assert.True(result.Services.ContainsKey("postgres"));

        var postgresService = result.Services["postgres"];
        Assert.Equal("postgres:17", postgresService.Image);
        Assert.Contains("5432:5432", postgresService.Ports);
    }
    [Fact]
        public void Generate_WithNullPort_ReturnsServiceWithoutPorts()
        {
            // Arrange
            var config = new EnvironmentConfiguration
            {
                Services = new List<ServiceConfiguration>
                {
                    new ServiceConfiguration
                    {
                        Name = "redis",
                        Image = "redis:8",
                        Port = null
                    }
                }
            };

            // Act
            var generator = new ComposeGenerator();
            var result = generator.Generate(config);

            // Assert
            Assert.NotNull(result.Services);
            Assert.Single(result.Services);
            Assert.True(result.Services.ContainsKey("redis"));

            var redisService = result.Services["redis"];
            Assert.Equal("redis:8", redisService.Image);
            Assert.Empty(redisService.Ports); 
        }
        [Fact]
        public void Generate_WithNullImage_ThrowsArgumentException()
        {
        
        }
    
    
}
