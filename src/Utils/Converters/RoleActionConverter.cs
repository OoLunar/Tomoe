using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using static Tomoe.Api.Moderation;

namespace Tomoe.Utils.Converters
{
    public class RoleActionConverter : IArgumentConverter<RoleAction>
    {
        public Task<Optional<RoleAction>> ConvertAsync(string value, CommandContext ctx) => value.ToLowerInvariant() switch
        {
            "mute" => Task.FromResult(Optional.FromValue(RoleAction.Mute)),
            "antimeme" or "anti_meme" => Task.FromResult(Optional.FromValue(RoleAction.Antimeme)),
            "voiceban" or "voice_ban" => Task.FromResult(Optional.FromValue(RoleAction.Voiceban)),
            _ => Task.FromResult(Optional.FromNoValue<RoleAction>())
        };
    }
}