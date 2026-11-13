using DevEnv.Application.Environments.Abstractions;
using global::DevEnv.Application.Environments.Abstractions;

namespace DevEnv.Infrastructure.Files
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
