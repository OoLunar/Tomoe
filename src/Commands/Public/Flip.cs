using System;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Flip : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.Flip");
		private static readonly Random _random = new();

		[Command("flip"), Description("A simple heads or tails command."), Aliases(new[] { "choose", "pick" })]
		public async Task Choose(CommandContext context)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_logger.Trace("Generating number between 0 and 1...");
			int randomNumber = _random.Next(0, 2);
			_logger.Trace($"Random number is {randomNumber}...");
			_ = Program.SendMessage(context, randomNumber == 0 ? "Heads" : "Tails");
			_logger.Trace("Message sent!");
		}

		[Command("flip")]
		public async Task Choose(CommandContext context, [Description("Have Tomoe pick from the choices listed.")] params string[] choices)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_logger.Debug($"Generating number between 1 and {choices.Length}...");
			int randomNumber = _random.Next(0, 1);
			_logger.Trace($"Random number is {randomNumber}...");
			_ = Program.SendMessage(context, choices[_random.Next(0, choices.Length)]);
			_logger.Trace("Message sent!");
		}
	}
}
