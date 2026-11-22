using System;
using Dciex.Core.Configuration;
using Dciex.Application.Configuration;
using YamlDotNet.Core;

namespace Dciex.Infrastructure.Configuration;

public class YamlConfigurationReader  : IConfigurationReader
{
  public async Task<EnvironmentConfiguration> LoadAsync(string path, CancellationToken cancellationToken = default)
    {
        var yamlContent = await File.ReadAllTextAsync(path, cancellationToken);
        if (string.IsNullOrWhiteSpace(yamlContent))
        {
            throw new InvalidDataException("YAML content is empty.");
        }
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();
        EnvironmentConfiguration? config;
        try
        {
            config = deserializer.Deserialize<EnvironmentConfiguration>(yamlContent);
        }
        catch (YamlException ex)
        {
            throw new ConfigurationReadException(
                "Configuration file contains invalid YAML.",
                ex);
        }

        if(config == null)
           throw new InvalidDataException("Failed to deserialize YAML content.");
        
        return config;
    }
}
