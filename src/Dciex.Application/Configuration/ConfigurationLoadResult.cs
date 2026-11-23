using Dciex.Core.Configuration;

namespace Dciex.Application.Configuration
{
    public class ConfigurationLoadResult
    {
        public EnvironmentConfiguration? Configuration { get; init; }
        public IReadOnlyList<ConfigurationValidationError> ValidationErrors { get; init; } = [];
        public IReadOnlyList<ConfigurationLoadError> LoadErrors {get; init;} = [];
        public bool IsValid => ValidationErrors.Count == 0 && Configuration != null && LoadErrors.Count == 0;


    }
}
