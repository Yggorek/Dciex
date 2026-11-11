using DevEnv.Core.Configuration;

namespace DevEnv.Application.Configuration
{
    public class ConfigurationLoadResult
    {
        public EnvironmentConfiguration? Configuration { get; init; }
        public IReadOnlyList<ConfigurationValidationError> ValidationErrors { get; init; } = Array.Empty<ConfigurationValidationError>();
        public bool IsValid => ValidationErrors.Count == 0 && Configuration != null;

    }
}
