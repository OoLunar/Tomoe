using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public
{
	public class Flip : BaseCommandModule
	{
		private const string _COMMAND_NAME = "flip";
		private const string _COMMAND_DESC = "A simple heads or tails command.";
		private const string _ARG_CHOICES_DESC = "Have Tomoe pick from the choices listed.";

		[Command(_COMMAND_NAME), Description(_COMMAND_DESC), Aliases(new string[] { "choose", "pick" })]
		public async Task Choose(CommandContext context) => Program.SendMessage(context, new System.Random().Next(0, 1) == 0 ? "Heads" : "Tails");

		[Command(_COMMAND_NAME)]
		public async Task Choose(CommandContext context, [Description(_ARG_CHOICES_DESC)] params string[] choices) => Program.SendMessage(context, choices[new System.Random().Next(0, choices.Length)]);
	}
}
