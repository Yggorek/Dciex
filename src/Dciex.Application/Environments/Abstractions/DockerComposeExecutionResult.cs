namespace Dciex.Application.Environments.Abstractions;

public sealed class DockerComposeExecutionResult
{
    public DockerComposeExecutionResult(int exitCode, string standardOutput, string standardError)
    {
        ExitCode = exitCode;
        StandardOutput = standardOutput;
        StandardError = standardError;
    }

    public int ExitCode { get; }

    public string StandardOutput { get; }

    public string StandardError { get; }

    public bool IsSuccess => ExitCode == 0;
}
