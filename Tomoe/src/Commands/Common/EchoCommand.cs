using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class EchoCommand : BaseCommandModule
    {
        [Command("echo")]
        [Description("Echoes the specified message.")]
        public Task EchoAsync(CommandContext context, params string[] messages)
        {
            if (messages.Length == 0 || messages.All(str => string.IsNullOrWhiteSpace(str)))
            {
                return context.RespondAsync("Please provide something for me to echo. Make sure there's no empty or whitespace-only arguments.");
            }

            DiscordMessageBuilder messageBuilder = new() { Content = string.Concat(messages) };
            messageBuilder.WithAllowedMentions(Mentions.None);
            return context.RespondAsync(messageBuilder);
        }
    }
}
