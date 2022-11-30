using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Net.Serialization;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class RawCommand : BaseCommand
    {
        [Command("raw")]
        public static Task ExecuteAsync(CommandContext context, DiscordMessage? message = null, bool jsonfied = false)
        {
            if (!context.IsSlashCommand && message is null)
            {
                message = context.Message!.ReferencedMessage;
            }

            if (message is null)
            {
                return context.ReplyAsync(new() { Content = "No message provided. Please give a link or reply to a message to use this command." });
            }

            DiscordMessageBuilder messageBuilder = new();
            if (jsonfied)
            {
                messageBuilder.AddFile("Message.json", new MemoryStream(Encoding.UTF8.GetBytes(DiscordJson.SerializeObject(message))));
                return context.ReplyAsync(messageBuilder);
            }

            if (message.Content.Length != 0)
            {
                string escapedContent = Formatter.Sanitize(message.Content);
                if (escapedContent.Length > 2000)
                {
                    messageBuilder.AddFile("Message.md", new MemoryStream(Encoding.UTF8.GetBytes(message.Content)));
                }
                else
                {
                    messageBuilder.WithContent(escapedContent);
                }
            }

            if (message.Embeds.Count != 0)
            {
                for (int i = 0; i < message.Embeds.Count; i++)
                {
                    DiscordEmbed embed = message.Embeds[i];
                    messageBuilder.AddFile($"Embed {i + 1}.json", new MemoryStream(Encoding.UTF8.GetBytes(DiscordJson.SerializeObject(embed))));
                }
            }

            return context.ReplyAsync(messageBuilder);
        }
    }
}
