using System;
using DevEnv.Core.Configuration;
using DevEnv.Application.Configuration;

namespace DevEnv.Infrastructure.Configuration;

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
        var config = deserializer.Deserialize<EnvironmentConfiguration>(yamlContent);
        if(config == null)
           throw new InvalidDataException("Failed to deserialize YAML content.");
        
        return config;
    }
}
