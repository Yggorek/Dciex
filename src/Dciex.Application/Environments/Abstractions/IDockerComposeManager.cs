namespace Dciex.Application.Environments.Abstractions;

public interface IDockerComposeManager
{
    Task<DockerComposeExecutionResult> StartAsync(
        string composeFilePath,
        string workingDirectory,
        CancellationToken cancellationToken = default);

    Task<DockerComposeExecutionResult> StopAsync(
        string composeFilePath,
        string workingDirectory,
        CancellationToken cancellationToken = default);
}
