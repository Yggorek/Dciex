using Dciex.Application.Compose;
using Dciex.Infrastructure.Compose;

namespace Dciex.Tests.Compose
{
    public class ComposeFinderTests
    {
        private readonly ComposeFinder _finder = new();

        [Fact]
        public void Find_DirectoryDoesNotExist_ReturnsDirectoryNotFound()
        {
            // Arrange
            using var directory = new TemporaryDirectory();
            var missingDirectory = Path.Combine(directory.Path, "missing");
            // Act
            var result = _finder.Find(missingDirectory);

            // Assert
            Assert.Equal(ComposeDiscoveryStatus.DirectoryNotFound, result.Status);
            Assert.Empty(result.Paths);
        }

        [Fact]
        public void Find_NoComposeFiles_ReturnsNotFound()
        {
            // Arrange
            using var directory = new TemporaryDirectory();
            // Act
            var result = _finder.Find(directory.Path);

            // Assert
            Assert.Equal(ComposeDiscoveryStatus.NotFound, result.Status);
            Assert.Empty(result.Paths);
        }

        [Fact]
        public void Find_OneComposeFile_ReturnsFound()
        {
            // Arrange
            using var directory = new TemporaryDirectory();
            var composePath = Path.Combine(directory.Path, "compose.yml");
            const string composeContents = "services: [invalid yaml";
            File.WriteAllText(composePath, composeContents);
            // Act
            var result = _finder.Find(directory.Path);

            // Assert
            Assert.Equal(ComposeDiscoveryStatus.Found, result.Status);
            Assert.Equal(composePath, Assert.Single(result.Paths));
            Assert.Equal(composePath, result.SinglePath);
            Assert.Equal(composeContents, File.ReadAllText(composePath));
        }

        [Theory]
        [InlineData("compose.yaml")]
        [InlineData("compose.yml")]
        [InlineData("docker-compose.yaml")]
        [InlineData("docker-compose.yml")]
        public void Find_SupportedComposeFileName_ReturnsFound(string fileName)
        {
            // Arrange
            using var directory = new TemporaryDirectory();
            var composePath = Path.Combine(directory.Path, fileName);
            File.WriteAllText(composePath, "services: {}");

            // Act
            var result = _finder.Find(directory.Path);

            // Assert
            Assert.Equal(ComposeDiscoveryStatus.Found, result.Status);
            Assert.Equal(composePath, Assert.Single(result.Paths));
        }

        [Fact]
        public void Find_MultipleComposeFiles_ReturnsAmbiguous()
        {
            // Arrange
            using var directory = new TemporaryDirectory();
            var composePath = Path.Combine(directory.Path, "compose.yml");
            var dockerComposePath = Path.Combine(directory.Path, "docker-compose.yaml");
            File.WriteAllText(composePath, "services: {}");
            File.WriteAllText(dockerComposePath, "services: {}");

            // Act
            var result = _finder.Find(directory.Path);

            // Assert
            Assert.Equal(ComposeDiscoveryStatus.Ambiguous, result.Status);
            Assert.Equal(2, result.Paths.Count);
            Assert.Contains(composePath, result.Paths);
            Assert.Contains(dockerComposePath, result.Paths);
            Assert.Null(result.SinglePath);
        }

        [Fact]
        public void Find_ComposeFileWithDifferentCase_ReturnsFound()
        {
            // Arrange
            using var directory = new TemporaryDirectory();
            var composePath = Path.Combine(directory.Path, "COMPOSE.YML");
            File.WriteAllText(composePath, "services: {}");
            // Act
            var result = _finder.Find(directory.Path);

            // Assert
            Assert.Equal(ComposeDiscoveryStatus.Found, result.Status);
            Assert.Equal(composePath, Assert.Single(result.Paths));
        }

        [Fact]
        public void Find_UnrelatedYamlFile_ReturnsNotFound()
        {
            // Arrange
            using var directory = new TemporaryDirectory();
            File.WriteAllText(Path.Combine(directory.Path, "settings.yaml"), "name: test");
            // Act
            var result = _finder.Find(directory.Path);

            // Assert
            Assert.Equal(ComposeDiscoveryStatus.NotFound, result.Status);
            Assert.Empty(result.Paths);
        }

        [Fact]
        public void Find_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            using var directory = new TemporaryDirectory();
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            // Act & Assert
            Assert.Throws<OperationCanceledException>(
                () => _finder.Find(directory.Path, cancellationTokenSource.Token));
        }

        [Fact]
        public void Find_InvalidPath_ThrowsArgumentException()
        {
            // Arrange
            const string invalidPath = "\0";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _finder.Find(invalidPath));
        }

        private sealed class TemporaryDirectory : IDisposable
        {
            public TemporaryDirectory()
            {
                Path = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    $"dciex-{Guid.NewGuid():N}");
                Directory.CreateDirectory(Path);
            }

            public string Path { get; }

            public void Dispose()
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
