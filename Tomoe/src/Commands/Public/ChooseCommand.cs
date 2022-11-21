using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace Tomoe.Commands.Common
{
    public sealed partial class ChooseCommand : ApplicationCommandModule
    {
        [SlashCommand("choose", "Choose from the options you provide. If none are given, it'll flip a coin!")]
        public static Task ChooseAsync(InteractionContext context, [Option("choice_num", "A list of items to choose from."), ParameterLimit(0, 25)] params string[] choices)
        {
            if (choices.Length == 0)
            {
                choices = new[] { "Heads", "Tails" };
            }

            return context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
            {
                Content = choices[Random.Shared.Next(0, choices.Length)]
            });
        }
    }
}
