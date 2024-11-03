using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using DSharpPlus.Net.Serialization;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// I like it raw.
    /// </summary>
    public static class RawCommand
    {
        /// <summary>
        /// Returns the raw content of a message.
        /// </summary>
        /// <param name="message">The message to get the raw content of.</param>
        /// <param name="jsonfied">Whether to return the raw content as JSON.</param>
        [Command("raw"), TextAlias("print"), DisplayName("Raw"), SlashCommandTypes(DiscordApplicationCommandType.SlashCommand, DiscordApplicationCommandType.MessageContextMenu), RequirePermissions(DiscordPermissions.ReadMessageHistory | DiscordPermissions.EmbedLinks, DiscordPermissions.None)]
        public static ValueTask ExecuteAsync(CommandContext context, DiscordMessage? message = null, bool jsonfied = false)
        {
            if (context is TextCommandContext textContext && message is null)
            {
                message = textContext.Message.ReferencedMessage;
            }

            if (message is null)
            {
                return context.RespondAsync("No message provided. Please give a link or reply to a message to use this command.");
            }

            DiscordMessageBuilder messageBuilder = new();
            if (jsonfied)
            {
                messageBuilder.AddFile("message.json", new MemoryStream(Encoding.UTF8.GetBytes(DiscordJson.SerializeObject(message))));
                return context.RespondAsync(messageBuilder);
            }

            if (message.Content?.Length is not null and not 0)
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

            return context.RespondAsync(messageBuilder);
        }
    }
}
