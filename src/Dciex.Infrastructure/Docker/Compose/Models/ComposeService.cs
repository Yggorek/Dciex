using YamlDotNet.Serialization;

namespace Dciex.Infrastructure.Docker.Compose.Models;


public class ComposeService
{
    [YamlMember(Alias = "image")]
    public required string Image { get; set; }

    [YamlMember(Alias = "ports")]
    public List<string> Ports { get; set; } = new List<string>();
}