namespace Tomoe.Commands.Public
{
    using DSharpPlus.SlashCommands;
    using System;
    using System.Threading.Tasks;

    public class Flip : SlashCommandModule
    {
        private static readonly Random _random = new();

        [SlashCommand("flip", "Choose from the options you provide!")]
        public async Task Overload(InteractionContext context, [Option("Choices", "A list of items to choose from.")] string choices = "Heads Tails") => await Program.SendMessage(context, choices.Split(' ')[_random.Next(0, choices.Split(' ').Length)]);
    }
}
