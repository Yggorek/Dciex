using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace DevEnv.Infrastructure.Docker.Compose;

public class ComposeEventEmitter(IEventEmitter next) : ChainedEventEmitter(next)
{
    public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
    {
        if (eventInfo.Source.Type == typeof(string) &&
            eventInfo.Source.Value is string value &&
            System.Text.RegularExpressions.Regex.IsMatch(
            value,
            @"^\d+:\d+$"))
            {
               eventInfo.Style = ScalarStyle.DoubleQuoted;
            }

        base.Emit(eventInfo, emitter);
    }
}