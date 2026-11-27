using Dciex.Infrastructure.Docker.Compose.Models;
using Dciex.Infrastructure.Docker.Compose;
using Dciex.Core.Configuration;

namespace Dciex.Tests.Docker.Compose
{
    public class ComposeSerializerTests
    {
        [Fact]
        public void Serialize_WithValidConfiguration_ReturnsYaml()
        {
            // Arrange
            var config = new ComposeConfiguration
            {
                Services = new Dictionary<string, ComposeService>
                {
                    {
                        "postgres", new ComposeService
                        {
                            Image = "postgres:17",
                            Ports = new List<string> { "5432:5432" }
                        }
                    },
                    
                }
            };
            var serializer = new ComposeSerializer();
            

            // Act
            var result = serializer.Serialize(config);

            // Assert
            Assert.Contains("services:", result);
            Assert.Contains("postgres:", result);
            Assert.Contains("image: postgres:17", result);
            Assert.Contains("ports:", result);
            Assert.Contains("5432:5432", result);
        }

        
    }
}
