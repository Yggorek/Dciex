using Dciex.Core.Configuration;

namespace Dciex.Application.Environments.Abstractions
{
    public interface IComposeFileGenerator
    {
        string Generate(EnvironmentConfiguration configuration);
    }
}
