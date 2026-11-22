using Dciex.Application.Environments.Abstractions;
using System.ComponentModel;
using System.Diagnostics;

namespace Dciex.Infrastructure.Docker.Compose;

public class DockerComposeManager : IDockerComposeManager
{
    private static readonly TimeSpan CancellationExitTimeout = TimeSpan.FromSeconds(5);

    public Task<DockerComposeExecutionResult> StartAsync(
        string composeFilePath,
        string workingDirectory,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(composeFilePath, workingDirectory, ["up", "-d"], cancellationToken);

    public Task<DockerComposeExecutionResult> StopAsync(
        string composeFilePath,
        string workingDirectory,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(composeFilePath, workingDirectory, ["down"], cancellationToken);

    private static async Task<DockerComposeExecutionResult> ExecuteAsync(
        string composeFilePath,
        string workingDirectory,
        IEnumerable<string> operationArguments,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(composeFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);
        cancellationToken.ThrowIfCancellationRequested();

        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("compose");
        startInfo.ArgumentList.Add("-f");
        startInfo.ArgumentList.Add(composeFilePath);
        foreach (var argument in operationArguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };
        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException("Process.Start returned false.");
            }
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                "Docker CLI could not be started. Make sure Docker is installed and available in PATH.",
                ex);
        }

        // Start draining both pipes before waiting for the process so neither can fill up and block it.
        var standardOutputTask = process.StandardOutput.ReadToEndAsync();
        var standardErrorTask = process.StandardError.ReadToEndAsync();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException cancellationException)
        {
            Exception? terminationError = null;
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException ex)
            {
                // Kill can race with process exit. Only treat this as a termination failure if it is still running.
                if (!process.HasExited)
                {
                    terminationError = ex;
                }
            }
            catch (Win32Exception ex)
            {
                terminationError = ex;
            }

            try
            {
                await process.WaitForExitAsync().WaitAsync(CancellationExitTimeout);
            }
            catch (TimeoutException timeoutException)
            {
                ObserveWhenFaulted(standardOutputTask);
                ObserveWhenFaulted(standardErrorTask);
                var errors = new List<Exception> { cancellationException };
                if (terminationError is not null)
                {
                    errors.Add(terminationError);
                }

                errors.Add(timeoutException);
                throw new OperationCanceledException(
                    "The operation was canceled, but the Docker process did not exit after termination was requested.",
                    new AggregateException(errors),
                    cancellationToken);
            }

            await Task.WhenAll(standardOutputTask, standardErrorTask);
            if (terminationError is not null)
            {
                throw new OperationCanceledException(
                    "The operation was canceled, but the Docker process could not be terminated cleanly.",
                    new AggregateException(cancellationException, terminationError),
                    cancellationToken);
            }

            throw;
        }

        await Task.WhenAll(standardOutputTask, standardErrorTask);
        return new DockerComposeExecutionResult(
            process.ExitCode,
            await standardOutputTask,
            await standardErrorTask);
    }

    private static void ObserveWhenFaulted(Task task) =>
        _ = task.ContinueWith(
            static faultedTask => _ = faultedTask.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
}
