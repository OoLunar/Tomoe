using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Flip : BaseCommandModule
	{
		private static readonly Logger Logger = new("Commands.Public.Flip");
		private static readonly Random Random = new();

		[Command("flip"), Description("A simple heads or tails command."), Aliases(new[] { "choose", "pick" })]
		public async Task Choose(CommandContext context)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			Logger.Trace("Generating number between 0 and 1...");
			int randomNumber = Random.Next(0, 2);
			Logger.Trace($"Random number is {randomNumber}...");
			_ = Program.SendMessage(context, randomNumber == 0 ? "Heads" : "Tails");
			Logger.Trace("Message sent!");
		}

		[Command("flip")]
		public async Task Choose(CommandContext context, [Description("Have Tomoe pick from the choices listed.")] params string[] choices)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			Logger.Debug($"Generating number between 1 and {choices.Length}...");
			int randomNumber = Random.Next(0, 1);
			Logger.Trace($"Random number is {randomNumber}...");
			_ = Program.SendMessage(context, choices[Random.Next(0, choices.Length)]);
			Logger.Trace("Message sent!");
		}
	}
}
