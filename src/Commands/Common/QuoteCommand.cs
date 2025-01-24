using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Tattle-tale.
    /// </summary>
    public static class QuoteCommand
    {
        /// <summary>
        /// Links a message from another channel.
        /// </summary>
        /// <param name="message">The message to repeat.</param>
        [Command("quote"), TextAlias("rat", "mock", "q", "tattle-tale", "tattle"), RequirePermissions([DiscordPermission.EmbedLinks, DiscordPermission.ReadMessageHistory], [])]
        public static ValueTask ExecuteAsync(CommandContext context, DiscordMessage message)
        {
            if (!message.Channel!.PermissionsFor(context.Member!).HasPermission(DiscordPermission.ViewChannel))
            {
                return context.RespondAsync("You don't have access to that message!");
            }

            string content = $"{message.Author!.Mention}: {message.Content}";
            if (message.ReferencedMessage is not null)
            {
                content = $"> {message.ReferencedMessage.Author!.Mention}: {message.ReferencedMessage.Content}\n{content}";
            }

            DiscordMessageBuilder messageBuilder = new();
            messageBuilder.AddEmbed(new DiscordEmbedBuilder()
            {
                Color = (message.Author as DiscordMember)?.Color,
                Description = content,
                Timestamp = message.CreationTimestamp,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    // We can't mention the channel or use timestamps here since the footer doesn't format anything.
                    Text = $"#{message.Channel.Name}"
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
