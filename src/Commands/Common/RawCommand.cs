using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.CommandAll.Processors.TextCommands.Attributes;
using DSharpPlus.CommandAll.Processors.TextCommands.ContextChecks;
using DSharpPlus.Entities;
using DSharpPlus.Net.Serialization;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class RawCommand
    {
        [Command("raw"), TextAlias("print"), SlashCommandTypes(ApplicationCommandType.SlashCommand, ApplicationCommandType.MessageContextMenu)]
        public static async Task ExecuteAsync(CommandContext context, [TextMessageReply] DiscordMessage? message = null, bool jsonfied = false)
        {
            if (context is TextCommandContext textContext && message is null)
            {
                message = textContext.Message.ReferencedMessage;
            }

            if (message is null)
            {
                await context.RespondAsync("No message provided. Please give a link or reply to a message to use this command.");
                return;
            }

            DiscordMessageBuilder messageBuilder = new();
            if (jsonfied)
            {
                messageBuilder.AddFile("Message.json", new MemoryStream(Encoding.UTF8.GetBytes(DiscordJson.SerializeObject(message))));
                await context.RespondAsync(messageBuilder);
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

            await context.RespondAsync(messageBuilder);
        }
    }
}
