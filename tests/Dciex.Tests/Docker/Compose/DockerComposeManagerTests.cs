using System.Diagnostics;
using Dciex.Infrastructure.Docker.Compose;

namespace Dciex.Tests.Docker.Compose;

[Collection("Docker compose manager")]
public sealed class DockerComposeManagerTests
{
    [Theory]
    [InlineData(true, "compose", "up", "-d")]
    [InlineData(false, "compose", "down")]
    public async Task UsesComposeFileAndExpectedArguments(bool start, params string[] operationArguments)
    {
        using var fakeDocker = new FakeDockerProcess();
        var composePath = Path.Combine(fakeDocker.WorkingDirectory, "compose file; (dev).yaml");
        var manager = new DockerComposeManager();

        var result = start
            ? await manager.StartAsync(composePath, fakeDocker.WorkingDirectory)
            : await manager.StopAsync(composePath, fakeDocker.WorkingDirectory);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("fake stderr" + Environment.NewLine, result.StandardError);
        Assert.Equal(
            new[] { "compose", "-f", composePath }.Concat(operationArguments.Skip(1)),
            result.StandardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
    }

    [Fact]
    public async Task ReturnsExitCodeAndBothOutputStreamsWhenComposeFails()
    {
        using var fakeDocker = new FakeDockerProcess();
        await File.WriteAllTextAsync(Path.Combine(fakeDocker.WorkingDirectory, "fake-docker-exit-code"), "23");

        var result = await new DockerComposeManager().StartAsync(
            Path.Combine(fakeDocker.WorkingDirectory, "compose.yaml"),
            fakeDocker.WorkingDirectory);

        Assert.False(result.IsSuccess);
        Assert.Equal(23, result.ExitCode);
        Assert.Contains("compose", result.StandardOutput);
        Assert.Equal("fake stderr" + Environment.NewLine, result.StandardError);
    }

    [Fact]
    public async Task CancellationStopsTheProcessAndRemainsCancellation()
    {
        using var fakeDocker = new FakeDockerProcess();
        await File.WriteAllTextAsync(Path.Combine(fakeDocker.WorkingDirectory, "fake-docker-delay"), "delay");
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new DockerComposeManager().StartAsync(
                Path.Combine(fakeDocker.WorkingDirectory, "compose.yaml"),
                fakeDocker.WorkingDirectory,
                cancellation.Token));

        var processId = int.Parse(await File.ReadAllTextAsync(
            Path.Combine(fakeDocker.WorkingDirectory, "fake-docker-pid")));
        await AssertProcessExitedAsync(processId);
    }

    private static async Task AssertProcessExitedAsync(int processId)
    {
        var timeout = Stopwatch.StartNew();
        while (timeout.Elapsed < TimeSpan.FromSeconds(5))
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                if (process.HasExited)
                {
                    return;
                }
            }
            catch (ArgumentException)
            {
                return;
            }

            await Task.Delay(25);
        }

        Assert.Fail($"FakeDocker process {processId} did not exit after cancellation.");
    }

    [Fact]
    public async Task WrapsDockerCliLaunchFailureAndPreservesCause()
    {
        var oldPath = Environment.GetEnvironmentVariable("PATH");
        var emptyPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(emptyPath);
        Environment.SetEnvironmentVariable("PATH", emptyPath);

        try
        {
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new DockerComposeManager().StartAsync("compose.yaml", emptyPath));

            Assert.NotNull(error.InnerException);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", oldPath);
            Directory.Delete(emptyPath, recursive: true);
        }
    }

    [Fact]
    public async Task PreCanceledOperationDoesNotStartDocker()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new DockerComposeManager().StartAsync("compose.yaml", ".", cancellation.Token));
    }

    private sealed class FakeDockerProcess : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        private readonly string? _oldPath;

        public FakeDockerProcess()
        {
            WorkingDirectory = Path.Combine(_root, "project with spaces");
            Directory.CreateDirectory(WorkingDirectory);
            var helperDirectory = Path.Combine(AppContext.BaseDirectory, "FakeDocker");

            foreach (var file in Directory.EnumerateFiles(helperDirectory))
            {
                var name = Path.GetFileName(file);
                File.Copy(file, Path.Combine(_root, name));
            }

            _oldPath = Environment.GetEnvironmentVariable("PATH");
            Environment.SetEnvironmentVariable("PATH", _root + Path.PathSeparator + _oldPath);
        }

        public string WorkingDirectory { get; }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("PATH", _oldPath);
            Directory.Delete(_root, recursive: true);
        }
    }
}

[CollectionDefinition("Docker compose manager", DisableParallelization = true)]
public sealed class DockerComposeManagerCollection;
