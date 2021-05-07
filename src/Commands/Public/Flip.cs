namespace Tomoe.Commands.Public
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using System;
    using System.Threading.Tasks;

    public class Flip : BaseCommandModule
    {
        private static readonly Random _random = new();

        [Command("flip"), Description("A simple heads or tails command."), Aliases("choose", "pick")]
        public async Task Overload(CommandContext context) => await Program.SendMessage(context, _random.Next(0, 2) == 0 ? "Heads" : "Tails");

        [Command("flip")]
        public async Task Overload(CommandContext context, [Description("Have Tomoe pick from the choices listed.")] params string[] choices) => await Program.SendMessage(context, choices[_random.Next(0, choices.Length)]);
    }
}
