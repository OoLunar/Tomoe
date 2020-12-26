using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public
{
	public class Repeat : BaseCommandModule
	{
		[Command("repeat"), Description("Repeats the command multiple times with the arguments provided. Waits 5 seconds before repeating the command.")]
		public async Task Execute(CommandContext context, int repeatCount, string command, [RemainingText] string arguments)
		{
			CommandContext newContext = context.CommandsNext.CreateContext(context.Message, context.Prefix, context.CommandsNext.RegisteredCommands[command.ToLowerInvariant()], arguments);
			for (int i = 0; i < repeatCount; i++)
			{
				await Task.Run(async () => context.CommandsNext.ExecuteCommandAsync(newContext));
				await Task.Delay(TimeSpan.FromSeconds(2));
			}
		}
	}
}
