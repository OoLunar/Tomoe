using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public {
    public class Flip : BaseCommandModule {
        [Command("flip")]
        [Description("A simple heads or tails command.")]
        [Aliases(new string[] { "choose", "pick" })]
        public async Task Main(CommandContext context) => Program.SendMessage(context, new System.Random().Next(0, 1) == 0 ? "Heads" : "Tails");

        [Command("flip")]
        public async Task Main(CommandContext context, string choice1, string choice2) => Program.SendMessage(context, new System.Random().Next(0, 1) == 0 ? choice1 : choice2);
    }
}