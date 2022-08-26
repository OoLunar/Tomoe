using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class Raw : BaseCommandModule
    {
        [Command("raw"), Description("Gets the raw version of the message provided."), Aliases("source")]
        public Task RawAsync(CommandContext context, [Description("Should the message be JSONfied or should we attempt to escape it?")] bool jsonfied = false, [Description("The message id or jumplink to the message.")] DiscordMessage? message = null)
        {
            message ??= context.Message.ReferencedMessage;
            if (message is null)
            {
                return context.RespondAsync("No message provided. Please give a link or reply to a message to use this command.");
            }

            DiscordMessageBuilder messageBuilder = new();
            if (jsonfied)
            {
                messageBuilder.WithFile("Message.json", new MemoryStream(Encoding.UTF8.GetBytes(DSharpPlus.Net.Serialization.DiscordJson.SerializeObject(message))));
                return context.RespondAsync(messageBuilder);
            }

            if (message.Content.Length != 0)
            {
                string escapedContent = Formatter.Sanitize(message.Content);
                if (escapedContent.Length > 2000)
                {
                    messageBuilder.WithFile("Message.md", new MemoryStream(Encoding.UTF8.GetBytes(message.Content)));
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
                    messageBuilder.WithFile($"Embed {i + 1}.json", new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(embed, new JsonSerializerOptions()
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        WriteIndented = true
                    })));
                }
            }

            return context.RespondAsync(messageBuilder);
        }
    }
}
