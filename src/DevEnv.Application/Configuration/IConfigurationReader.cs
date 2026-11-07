using System;
using DevEnv.Core.Configuration;


namespace DevEnv.Application.Configuration;

public interface IConfigurationReader
{
   Task<EnvironmentConfiguration> LoadAsync(string path, CancellationToken cancellationToken = default);
}
