
using Dciex.Application.Environments.Abstractions;
using Dciex.Core.Configuration;

namespace Dciex.Infrastructure.Docker.Compose;

public class ComposeFileGenerator : IComposeFileGenerator
{
    private readonly ComposeGenerator _generator;
    private readonly ComposeSerializer _serializer;

    public ComposeFileGenerator(
        ComposeGenerator generator,
        ComposeSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(generator);
        ArgumentNullException.ThrowIfNull(serializer);

        _generator = generator;
        _serializer = serializer;
    }

    public string Generate(EnvironmentConfiguration configuration)
    {
        var composeConfiguration = _generator.Generate(configuration);

        return _serializer.Serialize(composeConfiguration);
    }
}
