using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Enums;
using DSharpPlus.Entities;
using DSharpPlus.Net.Serialization;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class RawCommand : BaseCommand
    {
        [Command("raw", "print")]
        public static Task ExecuteAsync(CommandContext context, [RequiredBy(RequiredBy.SlashCommand)] DiscordMessage? message = null, bool jsonfied = false)
        {
            if (!context.InvocationType.HasFlag(CommandInvocationType.SlashCommand) && message is null)
            {
                message = context.Message!.ReferencedMessage;
            }

            if (message is null)
            {
                return context.ReplyAsync("No message provided. Please give a link or reply to a message to use this command.");
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

        public override Task OnErrorAsync(CommandContext context, Exception exception) => context.ReplyAsync("Please make sure the message has text or an embed!");
    }
}
