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
        public async Task WriteAllTextAtomicAsync(string path, string content, CancellationToken cancellationToken = default)
        {
            string? file = null;
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(path);
                ArgumentNullException.ThrowIfNull(content);
                var directory = Path.GetDirectoryName(path);
                ArgumentNullException.ThrowIfNull(directory);
                Directory.CreateDirectory(directory);
                file = Path.Combine(directory, $"{Path.GetRandomFileName()}.tmp");
                await File.WriteAllTextAsync(file, content, cancellationToken);
                File.Move(file, path, true);
            }
            finally
            {
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            }


        }
    }

}
