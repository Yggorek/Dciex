
using Dciex.Application.Environments.Abstractions;
using System.ComponentModel;
using System.Diagnostics;

namespace Dciex.Infrastructure.Docker.Compose;

public class DockerComposeManager : IDockerComposeManager
{
    public async Task StartAsync(
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "compose up -d",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = new Process
        {
            StartInfo = startInfo
        };

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException(
                    "Failed to start Docker process.");
            }
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException(
                "Docker CLI was not found. Make sure Docker is installed and available in PATH.",
                ex);
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException) when (process.HasExited)
            {
                // Process has already exited.
            }

            await process.WaitForExitAsync();
            await Task.WhenAll(outputTask, errorTask);

            throw;
        }

        await Task.WhenAll(outputTask, errorTask);

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Docker Compose failed: {error}");
        }
    }
}
