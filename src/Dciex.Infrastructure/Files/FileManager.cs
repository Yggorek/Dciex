using Dciex.Application.Environments.Abstractions;

namespace Dciex.Infrastructure.Files
{ 

    public class FileManager : IFileManager
    {
        public async Task WriteAllTextAsync(
            string path,
            string content,
            CancellationToken cancellationToken = default)
        {
            await File.WriteAllTextAsync(path, content, cancellationToken);
        }
    }

}
