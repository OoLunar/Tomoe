using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public static class HelpCommand
    {
        [Command("help")]
        public static ValueTask ExecuteAsync(CommandContext context, [RemainingText] string? command = null)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return context.RespondAsync(GetHelpMessage(context));
            }
            else if (context.Extension.Commands.TryGetValue(command, out Command? cmd))
            {
                Command? foundCommand = cmd;
                while (foundCommand is not null)
                {
                    throw new System.NotImplementedException();
                }

                return context.RespondAsync(GetHelpMessage(context, cmd));
            }
            else
            {
                return context.RespondAsync($"Command {command} not found.");
            }
        }

        public static DiscordMessageBuilder GetHelpMessage(CommandContext context)
        {
            DiscordMessageBuilder builder = new();
            return builder;
        }

        public static DiscordMessageBuilder GetHelpMessage(CommandContext context, Command command)
        {
            DiscordMessageBuilder builder = new();
            builder.WithContent($"Help command for {command.Name} is not implemented yet.");
            return builder;
        }
    }
}
