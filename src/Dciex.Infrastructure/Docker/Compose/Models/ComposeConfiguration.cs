using YamlDotNet.Serialization;

namespace Dciex.Infrastructure.Docker.Compose.Models;

public class ComposeConfiguration
{
    [YamlMember(Alias = "services")]
    public Dictionary<string, ComposeService> Services { get; set; } = new Dictionary<string, ComposeService>();
}