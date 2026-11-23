namespace Dciex.Application.Environments
{
    public class EnvironmentOperationError
    {
        public required string Code { get; init; }
        public required string Message { get; init; }
        public EnvironmentOperationErrorSource Source { get; init; }
        public string? PropertyName { get; init; }
    }

    public enum EnvironmentOperationErrorSource
    {
        ProjectRegistry,
        ConfigurationLoader,
        ConfigurationValidator,
        ComposeFileGenerator,
        FileManager,
        ComposeFinder,
        DockerComposeManager
    }
}
