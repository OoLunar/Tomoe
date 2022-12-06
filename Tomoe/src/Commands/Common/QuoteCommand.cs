using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;
using OoLunar.DSharpPlus.CommandAll.Commands.System.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class QuoteCommand : BaseCommand
    {
        [Command("quote", "rat", "mock")]
        public static Task ExecuteAsync(CommandContext context, DiscordMessage message)
        {
            if (!message.Channel.PermissionsFor(context.Member).HasPermission(Permissions.AccessChannels))
            {
                return context.ReplyAsync("You don't have access to that message!");
            }

            DiscordMessageBuilder messageBuilder = new();
            messageBuilder.AddEmbed(new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = message.Author.Username,
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
            return context.ReplyAsync(messageBuilder);
        }
    }
}
