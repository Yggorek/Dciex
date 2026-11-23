using Dciex.Application.Compose;

namespace Dciex.Infrastructure.Compose
{
    public class ComposeFinder : IComposeFinder
    {
        private static readonly string[] SupportedNames =
        [
            "compose.yaml",
            "compose.yml",
            "docker-compose.yaml",
            "docker-compose.yml"
        ];

        public ComposeDiscoveryResult Find(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(path);
            }
            catch (DirectoryNotFoundException)
            {
                return new ComposeDiscoveryResult
                {
                    Status = ComposeDiscoveryStatus.DirectoryNotFound,
                    Paths = []
                };
            }
            catch (FileNotFoundException)
            {
                return new ComposeDiscoveryResult
                {
                    Status = ComposeDiscoveryStatus.DirectoryNotFound,
                    Paths = []
                };
            }

            if ((attributes & FileAttributes.Directory) == 0)
            {
                return new ComposeDiscoveryResult
                {
                    Status = ComposeDiscoveryStatus.DirectoryNotFound,
                    Paths = []
                };
            }

            var files = Directory.EnumerateFiles(path);
            var foundFiles = new List<string>();
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileName = Path.GetFileName(file);
                if (SupportedNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                {
                    foundFiles.Add(file);
                }
            }

            if (foundFiles.Count > 1)
            {
                return new ComposeDiscoveryResult
                {
                    Status = ComposeDiscoveryStatus.Ambiguous,
                    Paths = foundFiles
                };
            }

            if (foundFiles.Count == 0)
            {
                return new ComposeDiscoveryResult
                {
                    Status = ComposeDiscoveryStatus.NotFound,
                    Paths = foundFiles
                };
            }

            return new ComposeDiscoveryResult
            {
                Status = ComposeDiscoveryStatus.Found,
                Paths = foundFiles
            };
        }
    }
}
