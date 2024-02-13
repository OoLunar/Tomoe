using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class ReverseCommand
    {
        private readonly HttpClient _httpClient;
        public ReverseCommand(HttpClient httpClient) => _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        [Command("reverse")]
        public async ValueTask ExecuteAsync(CommandContext context, DiscordAttachment? attachment = null)
        {
            string? message;
            if (attachment is null)
            {
                if (context is not TextCommandContext textCommandContext)
                {
                    await context.RespondAsync("Please provide a file to reverse.");
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
                await context.RespondAsync("Nothing to reverse.");
                return;
            }

            string reversedContent = string.Join('\n', message.Split('\n').Reverse());
            DiscordMessageBuilder builder = new();
            if (attachment is not null)
            {
                builder.AddFile($"{Path.GetFileNameWithoutExtension(attachment.FileName)}_reversed{Path.GetExtension(attachment.FileName)}", new MemoryStream(Encoding.UTF8.GetBytes(reversedContent)), AddFileOptions.CloseStream);
            }
            else if (reversedContent.Length > 1992)
            {
                builder.AddFile("reversed.txt", new MemoryStream(Encoding.UTF8.GetBytes(reversedContent)), AddFileOptions.CloseStream);
            }
            else
            {
                builder.WithContent($"```\n{reversedContent}\n```");
            }

            await context.RespondAsync(builder);
        }
    }
}
