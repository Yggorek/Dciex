using Dciex.Core.Configuration;

namespace Dciex.Application.Configuration;

public class ConfigurationValidator
{
   public IReadOnlyList<ConfigurationValidationError> Validate(EnvironmentConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        var errors = new List<ConfigurationValidationError>();

        ValidateEnvironment(configuration, errors);

        ValidateServices(configuration, errors);

        ValidateApplication(configuration, errors);
        
        return errors;
    }
    private static void ValidateEnvironment(EnvironmentConfiguration configuration, List<ConfigurationValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(configuration.Name))
            errors.Add(new ConfigurationValidationError
            {
                Code = "EnvironmentNameRequired",
                PropertyName = nameof(EnvironmentConfiguration.Name),
                ErrorMessage = "Environment name is required."
            });
        if (configuration.EnvironmentMode == null)
            errors.Add(new ConfigurationValidationError
            {
                Code = "EnvironmentModeRequired",
                PropertyName = nameof(EnvironmentConfiguration.EnvironmentMode),
                ErrorMessage = "Environment mode is required."
            });
        else if (!Enum.IsDefined(configuration.EnvironmentMode.Value))
            errors.Add(new ConfigurationValidationError
            {
                Code = "EnvironmentModeInvalid",
                PropertyName = nameof(EnvironmentConfiguration.EnvironmentMode),
                ErrorMessage = "Environment mode is not valid."
            });
    }
    private static void ValidateServices(EnvironmentConfiguration configuration, List<ConfigurationValidationError> errors)
    {
        if (configuration.Services != null)
        {
            for (int i = 0; i < configuration.Services.Count; i++)
            {
                var service = configuration.Services[i];

                if (service == null)
                {
                    errors.Add(new ConfigurationValidationError
                    {
                        Code = "ServiceElementNull",
                        PropertyName = $"Services[{i}]",
                        ErrorMessage = "Service configuration element cannot be null."
                    });
                    continue;
                }
                if (string.IsNullOrWhiteSpace(service.Name))
                    errors.Add(new ConfigurationValidationError
                    {
                        Code = "ServiceNameRequired",
                        PropertyName = $"Services[{i}].Name",
                        ErrorMessage = "Service name is required."
                    });
                if (string.IsNullOrWhiteSpace(service.Image))
                    errors.Add(new ConfigurationValidationError
                    {
                        Code = "ServiceImageRequired",
                        PropertyName = $"Services[{i}].Image",
                        ErrorMessage = "Service image is required."
                    });
                if (service.Port.HasValue && (service.Port.Value <= 0 || service.Port.Value > 65535))
                {
                    errors.Add(new ConfigurationValidationError
                    {
                        Code = "ServicePortOutOfRange",
                        PropertyName = $"Services[{i}].Port",
                        ErrorMessage = "Service port must be between 1 and 65535."
                    });
                }

            }

        }
    }
    private static void ValidateApplication(EnvironmentConfiguration configuration, List<ConfigurationValidationError> errors)
    {
        if (configuration.EnvironmentMode == EnvironmentMode.Managed &&
            configuration.Application == null)
        {
            errors.Add(new ConfigurationValidationError
            {
                Code = "ManagedApplicationRequired",
                PropertyName = nameof(EnvironmentConfiguration.Application),
                ErrorMessage = "Application configuration is required for managed environments."
            });
        }
        if (configuration.Application != null && string.IsNullOrWhiteSpace(configuration.Application.Path))
        {
            errors.Add(new ConfigurationValidationError
            {
                Code = "ApplicationPathRequired",
                PropertyName = "Application.Path",
                ErrorMessage = "Application path is required."
            });
        }
    }
}
