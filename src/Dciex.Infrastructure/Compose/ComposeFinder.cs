using Dciex.Application.Compose;

namespace Dciex.Infrastructure.Compose
{
    public class ComposeFinder : IComposeFinder
    {
        private static readonly string[] supportedNames = ["compose.yml", "compose.yaml", "docker-compose.yaml", "docker-compose.yml"];
        public ComposeDiscoveryResult Find(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Directory.Exists(path))
                return new ComposeDiscoveryResult
                {
                    Status = ComposeDiscoveryStatus.DirectoryNotFound,
                    Paths = []
                };
            

            var files = Directory.EnumerateFiles(path);
            List<string> foundFiles = new List<string>();
            foreach(var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileName = Path.GetFileName(file);
                if (supportedNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                    foundFiles.Add(file);
                
            }
            if(foundFiles.Count > 1)
                return new ComposeDiscoveryResult
                {
                    Status = ComposeDiscoveryStatus.Ambiguous,
                    Paths = foundFiles
                };
            else if(foundFiles.Count == 0)
                return new ComposeDiscoveryResult
                {
                    Status = ComposeDiscoveryStatus.NotFound,
                    Paths = foundFiles
                };
            return new ComposeDiscoveryResult
            {
                Status = ComposeDiscoveryStatus.Found,
                Paths = foundFiles
            };

        }
    }
}
