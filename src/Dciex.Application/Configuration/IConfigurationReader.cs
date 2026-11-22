using System;
using Dciex.Core.Configuration;


namespace Dciex.Application.Configuration;

public interface IConfigurationReader
{
   Task<EnvironmentConfiguration> LoadAsync(string path, CancellationToken cancellationToken = default);
}
