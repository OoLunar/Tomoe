namespace Tomoe.Commands.Public
{
    using DSharpPlus;
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
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "No guild icon set!",
                    IsEphemeral = true
                });
            }
            else
            {
                DiscordEmbedBuilder embedBuilder = new()
                {
                    Title = context.Guild.Name + (context.Guild.Name.EndsWith('s') ? "' Guild Icon" : "'s Guild Icon"),
                    ImageUrl = context.Guild.IconUrl.Replace(".jpg", ".png?size=1024"),
                    Color = new DiscordColor("#7b84d1")
                };
                DiscordInteractionResponseBuilder messageBuilder = new();
                messageBuilder.AddEmbed(embedBuilder);
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, messageBuilder);
            }
        }
    }
}
