
namespace Tomoe.Utils.Converters
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Converters;
    using DSharpPlus.Entities;
    using System.Threading.Tasks;
    using Tomoe.Commands.Moderation;

    public class RoleActionConverter : IArgumentConverter<Config.RoleAction>
    {
        public Task<Optional<Config.RoleAction>> ConvertAsync(string value, CommandContext ctx) => value.ToLowerInvariant() switch
        {
            "mute" => Task.FromResult(Optional.FromValue(Config.RoleAction.Mute)),
            "antimeme" or "anti_meme" => Task.FromResult(Optional.FromValue(Config.RoleAction.Antimeme)),
            "voiceban" or "voice_ban" => Task.FromResult(Optional.FromValue(Config.RoleAction.Voiceban)),
            _ => Task.FromResult(Optional.FromNoValue<Config.RoleAction>())
        };
    }
}
