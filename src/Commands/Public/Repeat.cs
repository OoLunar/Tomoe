using System;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Repeat : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.Repeat");

		[Command("repeat"), Description("Repeats the command multiple times with the arguments provided. Waits 5 seconds before repeating the command.")]
		public async Task Execute(CommandContext context, int repeatCount, string command, [RemainingText] string arguments)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			string commandName = command.ToLowerInvariant();
			_logger.Trace($"Creating context for \"{commandName}\"...");
			CommandContext newContext = context.CommandsNext.CreateContext(context.Message, context.Prefix, context.CommandsNext.RegisteredCommands[commandName], arguments);
			for (int i = 0; i < repeatCount; i++)
			{
				_logger.Trace($"#{i}, executing {commandName}...");
				_ = await Task.Run(async () => context.CommandsNext.ExecuteCommandAsync(newContext));
				_logger.Trace("Sleep for 2 seconds to avoid breaking rate limits...");
				await Task.Delay(TimeSpan.FromSeconds(2));
			}
			_logger.Trace($"Successfully executed {commandName} {repeatCount} {(repeatCount == 1 ? "time!" : "times!")}");
		}
	}
}