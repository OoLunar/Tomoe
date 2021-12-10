namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("guild_icon", "Gets the guild's icon.")]
        public static async Task GuildIcon(InteractionContext context)
        {
            if (context.Guild.IconUrl == null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "Error: The guild has no icon!"
                });
            }
            else
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = context.Guild.Name + (context.Guild.Name.EndsWith('s') ? "' Guild Icon" : "'s Guild Icon"),
                    ImageUrl = context.Guild.IconUrl.Replace(".jpg", ".png?size=1024"),
                    Color = new DiscordColor("#7b84d1")
                }));
            }
        }
    }
}
