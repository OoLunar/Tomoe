using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public {
    public class GuildIcon : BaseCommandModule {
        [Command("guild_icon")]
        [Description("Gets the guild's icon.")]
        [Aliases(new string[] { "guildicon", "guildpfp", "guild_pfp" })]
        public async Task Get(CommandContext context) => Program.SendMessage(context, (context.Guild.IconUrl ?? "No custom icon set.").Replace(".jpg", ".png?size=512"));
    }
}