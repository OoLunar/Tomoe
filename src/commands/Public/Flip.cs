using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public
{
	public class Flip : BaseCommandModule
	{
		[Command("flip"), Description("A simple heads or tails command."), Aliases(new[] { "choose", "pick" })]
		public async Task Choose(CommandContext context) => Program.SendMessage(context, new System.Random().Next(0, 1) == 0 ? "Heads" : "Tails");

		[Command("flip")]
		public async Task Choose(CommandContext context, [Description("Have Tomoe pick from the choices listed.")] params string[] choices) => Program.SendMessage(context, choices[new System.Random().Next(0, choices.Length)]);
	}
}
