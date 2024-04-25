using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public static class QuoteCommand
    {
        [Command("quote"), TextAlias("rat", "mock", "q"), RequireGuild]
        public static ValueTask ExecuteAsync(CommandContext context, DiscordMessage message)
        {
            if (!message.Channel!.PermissionsFor(context.Member!).HasPermission(DiscordPermissions.AccessChannels))
            {
                return context.RespondAsync("You don't have access to that message!");
            }

            DiscordMessageBuilder messageBuilder = new();
            messageBuilder.AddEmbed(new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = message.Author!.Username,
                    IconUrl = message.Author.AvatarUrl
                },
                Color = ((DiscordMember)message.Author).Color,
                Description = message.Content,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    // We can't mention the channel or use timestamps here since the footer doesn't format anything.
                    Text = $"#{message.Channel.Name} | {message.Timestamp.UtcDateTime:yyyy-MM-dd HH:mm:ss} UTC"
                }
            });

            foreach (DiscordAttachment attachment in message.Attachments)
            {
                messageBuilder.AddEmbed(new DiscordEmbedBuilder()
                {
                    ImageUrl = attachment.Url
                });
            }

            return context.RespondAsync(messageBuilder);
        }
    }
}
