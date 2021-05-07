namespace Tomoe.Commands.Public
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using System.Threading.Tasks;

    public class GuildIcon : BaseCommandModule
    {
        [Command("guild_icon"), Description("Gets the guild's icon."), Aliases("guild_pfp")]
        public async Task Overload(CommandContext context) => await Program.SendMessage(context, (context.Guild.IconUrl ?? "No custom icon set.").Replace(".jpg", ".png?size=1024"));
    }
}
