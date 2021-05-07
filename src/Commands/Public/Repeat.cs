namespace Tomoe.Commands.Public
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using System;
    using System.Threading.Tasks;

    public class Repeat : BaseCommandModule
    {
        [Command("repeat"), Description("Repeats the command multiple times with the arguments provided. Waits 5 seconds before repeating the command.")]
        public async Task Overload(CommandContext context, int repeatCount, string command, [RemainingText] string arguments)
        {
            string commandName = command.ToLowerInvariant();
            CommandContext newContext = context.CommandsNext.CreateContext(context.Message, context.Prefix, context.CommandsNext.RegisteredCommands[commandName], arguments);
            for (int i = 0; i < repeatCount; i++)
            {
                await Task.Run(async () => await context.CommandsNext.ExecuteCommandAsync(newContext));
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
