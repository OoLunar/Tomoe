using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Net.Serialization;
using DSharpPlus.SlashCommands;

namespace Tomoe.Commands.Common
{
    public sealed class RawCommand : ApplicationCommandModule
    {
        [SlashCommand("raw", "Gets the raw version of the message provided.")]
        public static Task RawAsync(InteractionContext context, [Option("message_link", "The message id or jumplink to the message.")] DiscordMessage message, [Option("jsonify", "Should the message be in JSON form or should we attempt to escape it?")] bool jsonfied = false)
        {
            DiscordInteractionResponseBuilder messageBuilder = new();
            if (jsonfied)
            {
                messageBuilder.AddFile("Message.json", new MemoryStream(Encoding.UTF8.GetBytes(DiscordJson.SerializeObject(message))));
                return context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, messageBuilder);
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

            return context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, messageBuilder);
        }
    }
}
