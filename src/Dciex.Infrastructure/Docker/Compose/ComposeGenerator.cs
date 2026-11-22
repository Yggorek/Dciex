using Dciex.Core.Configuration;
using Dciex.Infrastructure.Docker.Compose.Models;


namespace Dciex.Infrastructure.Docker.Compose;

public class ComposeGenerator
{
    public ComposeConfiguration Generate(EnvironmentConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var composeConfiguration = new ComposeConfiguration();
        
        var services = configuration.Services ?? [];
        foreach (var service in services)
        {
            if (string.IsNullOrWhiteSpace(service.Name) || string.IsNullOrWhiteSpace(service.Image))
            {
                throw new ArgumentException("Service name and image must be provided.");
            }
            if (service.Port.HasValue && (service.Port < 1 || service.Port > 65535))
            {
                throw new ArgumentOutOfRangeException(nameof(service.Port),service.Port,$"Port for service {service.Name} must be between 1 and 65535.");
            }
            
            var composeService = new ComposeService
            {
                Image = service.Image,
                Ports = service.Port.HasValue ? new List<string> { $"{service.Port.Value}:{service.Port.Value}" } : new List<string>()
            };
            composeConfiguration.Services.Add(service.Name, composeService);
        }   

        return composeConfiguration;
    }
}