using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// I can't believe it's not butter - CoPilot.
    /// </summary>
    public sealed class UnindentCommand
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new instance of <see cref="UnindentCommand"/>.
        /// </summary>
        /// <param name="httpClient">Required service for retrieving remote files.</param>
        public UnindentCommand(HttpClient httpClient) => _httpClient = httpClient;

        /// <summary>
        /// Removes the minimum indention from a message or file.
        /// </summary>
        /// <param name="attachment">The file to unindent.</param>
        [Command("unindent"), TextAlias("trim")]
        public async ValueTask ExecuteAsync(CommandContext context, DiscordAttachment? attachment = null)
        {
            string? message;
            if (attachment is null)
            {
                if (context is not TextCommandContext textCommandContext)
                {
                    await context.RespondAsync("Please provide a file to unindent.");
                    return;
                }

                message = textCommandContext.Message.ReferencedMessage is not null
                    ? textCommandContext.Message.ReferencedMessage.Content
                    : textCommandContext.Message.Content;
            }
            else
            {
                await context.DeferResponseAsync();
                message = await _httpClient.GetStringAsync(attachment.Url);
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                await context.RespondAsync("Nothing to unindent.");
                return;
            }

            string unindentedContent = Unindent(message);
            DiscordMessageBuilder builder = new();
            if (attachment is not null)
            {
                builder.AddFile(Path.GetFileNameWithoutExtension(attachment.FileName) + "_unindented" + Path.GetExtension(attachment.FileName), new MemoryStream(Encoding.UTF8.GetBytes(unindentedContent)), AddFileOptions.CloseStream);
            }
            else if (unindentedContent.Length > 1992)
            {
                builder.AddFile("unindented.txt", new MemoryStream(Encoding.UTF8.GetBytes(unindentedContent)), AddFileOptions.CloseStream);
            }
            else
            {
                builder.WithContent($"```\n{unindentedContent}\n```");
            }

            await context.RespondAsync(builder);
        }

        private static string Unindent(string str)
        {
            // Users cannot send tabs; they get converted to spaces.
            // However, a bot may send tabs, so convert them to 4 spaces first
            str = str.Replace("\t", "    ");
            int minIndent = GetMinIndent(str);
            if (minIndent == 0)
            {
                return str;
            }

            Regex regex = new($"^ {{0,{minIndent}}}", RegexOptions.Multiline);
            return regex.Replace(str, "");
        }

        private static int GetMinIndent(string str)
        {
            string[] lines = str.Split('\n');
            int minIndent = int.MaxValue;

            foreach (string line in lines)
            {
                string trimmed = line.TrimStart();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    minIndent = Math.Min(minIndent, line.Length - trimmed.Length);
                }
            }

            return minIndent;
        }
    }
}
