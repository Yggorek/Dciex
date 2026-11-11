using DevEnv.Infrastructure.Docker.Compose.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DevEnv.Infrastructure.Docker.Compose;

public class ComposeSerializer
{
    public string Serialize(ComposeConfiguration configuration)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithEventEmitter(next => new ComposeEventEmitter(next))
            .Build();

        return serializer.Serialize(configuration);
    }
}