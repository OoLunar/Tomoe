using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Repeat : BaseCommandModule
	{
		private static readonly Logger Logger = new("Commands.Public.Repeat");

		[Command("repeat"), Description("Repeats the command multiple times with the arguments provided. Waits 5 seconds before repeating the command.")]
		public async Task Execute(CommandContext context, int repeatCount, string command, [RemainingText] string arguments)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			Logger.Trace($"Creating context for \"{command.ToLower()}\"...");
			CommandContext newContext = context.CommandsNext.CreateContext(context.Message, context.Prefix, context.CommandsNext.RegisteredCommands[command.ToLowerInvariant()], arguments);
			for (int i = 0; i < repeatCount; i++)
			{
				Logger.Trace($"#{i}, executing {command.ToLower()}...");
				await Task.Run(async () => context.CommandsNext.ExecuteCommandAsync(newContext));
				Logger.Trace("Sleep for 2 seconds to avoid breaking rate limits...");
				await Task.Delay(TimeSpan.FromSeconds(2));
			}
			Logger.Trace($"Successfully executed {command.ToLower()} {repeatCount} times!");
		}
	}
}
