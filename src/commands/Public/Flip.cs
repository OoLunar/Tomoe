using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public {
    public class Flip : BaseCommandModule {
        [Command("flip")]
        [Description("Does a heads or tails for the user.")]
        public async Task Main(CommandContext context) => Tomoe.Program.SendMessage(context, new System.Random().Next(0, 1) == 0 ? "Heads" : "Tails");
    }
}