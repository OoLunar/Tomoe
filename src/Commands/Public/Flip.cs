namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Flip : SlashCommandModule
    {
        private static readonly Random _random = new();

        [SlashCommand("flip", "Choose from the options you provide!")]
        public async Task Overload(InteractionContext context, [Option("Choices", "A list of items to choose from.")] string choices = "Heads Tails")
        {
            List<string> list = choices.Split(' ').ToList();
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
            {
                Content = list[_random.Next(0, list.Count)]
            });
        }
    }
}
