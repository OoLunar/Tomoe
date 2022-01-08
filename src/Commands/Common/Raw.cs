using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tomoe.Commands.Common
{
    public class Raw : BaseCommandModule
    {
        [Command("raw"), Description("Gets the raw version of the message provided."), Aliases("source")]
        public async Task Overload(CommandContext context, [Description("The message id or jumplink to the message.")] DiscordMessage message)
        {
            DiscordMessageBuilder messageBuilder = new();
            if (message.Content.Length != 0)
            {
                if (message.Content.Length > 2000)
                {
                    messageBuilder.WithFile("Message.md", new MemoryStream(Encoding.UTF8.GetBytes(message.Content)));
                }
                else
                {
                    messageBuilder.WithContent(Formatter.Sanitize(message.Content));
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

            await context.RespondAsync(messageBuilder);
        }
    }
}