using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Tomoe.Commands
{
    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("guild_icon", "Gets the guild's icon.")]
        public static Task GuildIconAsync(InteractionContext context) => context.Guild.IconUrl == null
            ? context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() { Content = "Error: The guild has no icon!" })
            : context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
            {
                Title = context.Guild.Name + (context.Guild.Name.EndsWith('s') ? "' Guild Icon" : "'s Guild Icon"),
                ImageUrl = context.Guild.IconUrl.Replace(".jpg", ".png?size=1024"),
                Color = new DiscordColor("#7b84d1")
            }));
    }
}
