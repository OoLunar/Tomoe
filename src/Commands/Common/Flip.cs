using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Common
{
	public class Flip : BaseCommandModule
	{
		[Command("flip"), Description("A simple heads or tails command."), Aliases("choose", "pick")]
		public Task FlipAsync(CommandContext context)
			=> context.RespondAsync(Random.Shared.Next(0, 2) == 0 ? "Heads" : "Tails");

		[Command("flip")]
		public Task FlipAsync(CommandContext context, [Description("Provide a list of options to choose from.")] params string[] choices)
			=> context.RespondAsync(new DiscordMessageBuilder().WithContent(choices[Random.Shared.Next(0, choices.Length)]).WithAllowedMentions(Array.Empty<IMention>()));
	}
}
