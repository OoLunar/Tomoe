namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System.Threading.Tasks;

    public class BotInfo : SlashCommandModule
    {
        [SlashCommand("bot_info", "Gets general info about the bot.")]
        public async Task Overload(InteractionContext context)
        {
            DiscordEmbedBuilder discordEmbedBuilder = new()
            {
                Title = "Bot Info",
                Color = new DiscordColor("#7b84d1"),
                ImageUrl = context.Client.CurrentUser.GetAvatarUrl(ImageFormat.Png)
            };

            foreach ((Api.Public.BotInfoField field, string value) in Api.Public.BotInfo(context))
            {
                discordEmbedBuilder.AddField(field.Humanize(), value, true);
            }

            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(discordEmbedBuilder));
        }
    }
}
