using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public
{
	public class Flip : BaseCommandModule
	{
		private const string _COMMAND_NAME = "flip";
		private const string _COMMAND_DESC = "A simple heads or tails command.";
		private const string _ARG_CHOICE1_DESC = "Option 1 to pick from.";
		private const string _ARG_CHOICE2_DESC = "Option 2 to pick from.";

		[Command(_COMMAND_NAME), Description(_COMMAND_DESC), Aliases(new string[] { "choose", "pick" })]
		public async Task Choose(CommandContext context) => Program.SendMessage(context, new System.Random().Next(0, 1) == 0 ? "Heads" : "Tails");

		[Command(_COMMAND_NAME)]
		public async Task Choose(CommandContext context, [Description(_ARG_CHOICE1_DESC)] string choice1, [Description(_ARG_CHOICE2_DESC)] string choice2) => Program.SendMessage(context, new System.Random().Next(0, 1) == 0 ? choice1 : choice2);
	}
}
