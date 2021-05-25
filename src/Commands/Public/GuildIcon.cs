namespace Tomoe.Commands.Public
{
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class GuildIcon : SlashCommandModule
    {
        [SlashCommand("guild_icon", "Gets the guild's icon.")]
        public async Task Overload(InteractionContext context)
        {
            if (context.Guild.IconUrl == null)
            {
                await Program.SendMessage(context, "No guild icon set!");
            }
            else
            {
                // Create an embed because sending links in ephemeral message don't retrieve link embeds.
                await Program.SendMessage(context, null, new DiscordEmbedBuilder()
                {
                    Title = context.Guild.Name + (context.Guild.Name.EndsWith('s') ? "' Guild Icon" : "'s Guild Icon"),
                    ImageUrl = context.Guild.IconUrl.Replace(".jpg", ".png?size=1024"),
                    Color = new DiscordColor("#7b84d1")
                });
            }
        }
    }
}
