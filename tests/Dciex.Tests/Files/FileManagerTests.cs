using Dciex.Infrastructure.Files;

namespace Dciex.Tests.Files;

public sealed class FileManagerTests
{
    private readonly FileManager _fileManager = new();

    [Fact]
    public async Task WriteAllTextAtomicAsync_NewFile_CreatesFileWithContent()
    {
        // Arrange
        using var directory = new TemporaryDirectory();
        var filePath = Path.Combine(directory.Path, "new-file.txt");
        const string content = "new file content";

        // Act
        await _fileManager.WriteAllTextAtomicAsync(filePath, content);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.Equal(content, await File.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task WriteAllTextAtomicAsync_ExistingFile_ReplacesContent()
    {
        // Arrange
        using var directory = new TemporaryDirectory();
        var filePath = Path.Combine(directory.Path, "existing-file.txt");
        await File.WriteAllTextAsync(filePath, "original content");
        const string content = "replacement content";

        // Act
        await _fileManager.WriteAllTextAtomicAsync(filePath, content);

        // Assert
        Assert.Equal(content, await File.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task WriteAllTextAtomicAsync_MissingNestedDirectory_CreatesDirectoryAndFile()
    {
        // Arrange
        using var directory = new TemporaryDirectory();
        var nestedDirectory = Path.Combine(directory.Path, "missing", "nested");
        var filePath = Path.Combine(nestedDirectory, "file.txt");
        const string content = "nested file content";

        // Act
        await _fileManager.WriteAllTextAtomicAsync(filePath, content);

        // Assert
        Assert.True(Directory.Exists(nestedDirectory));
        Assert.True(File.Exists(filePath));
        Assert.Equal(content, await File.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task WriteAllTextAtomicAsync_SuccessfulWrite_LeavesNoTemporaryFiles()
    {
        // Arrange
        using var directory = new TemporaryDirectory();
        var filePath = Path.Combine(directory.Path, "file.txt");

        // Act
        await _fileManager.WriteAllTextAtomicAsync(filePath, "content");

        // Assert
        Assert.Empty(Directory.EnumerateFiles(directory.Path, "*.tmp"));
    }

    [Fact]
    public async Task WriteAllTextAtomicAsync_CanceledWrite_PreservesExistingContent()
    {
        // Arrange
        using var directory = new TemporaryDirectory();
        var filePath = Path.Combine(directory.Path, "existing-file.txt");
        const string originalContent = "original content";
        await File.WriteAllTextAsync(filePath, originalContent);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var exception = await Record.ExceptionAsync(
            () => _fileManager.WriteAllTextAtomicAsync(filePath, "replacement content", cancellationTokenSource.Token));

        // Assert
        Assert.IsAssignableFrom<OperationCanceledException>(exception);
        Assert.Equal(originalContent, await File.ReadAllTextAsync(filePath));
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
