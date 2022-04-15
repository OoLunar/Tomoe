using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Common
{
    public class GuildIcon : BaseCommandModule
    {
        [Command("guild_icon"), Description("Gets the guild's icon."), Aliases("server_icon")]
        public async Task GuildIconAsync(CommandContext context) => await context.RespondAsync((context.Guild.IconUrl ?? "No custom guild icon set.").Replace(".jpg", ".png?size=4096"));
    }
}
