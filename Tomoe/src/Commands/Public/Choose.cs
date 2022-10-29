using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace Tomoe.Commands
{
    public partial class Public : ApplicationCommandModule
    {
        public static Regex RegexArgumentParser { get; private set; } = RegexArgumentParserMethod();

        [SlashCommand("choose", "Choose from the options you provide. If none are given, it'll flip a coin!")]
        public static Task ChooseAsync(InteractionContext context, [Option("Choices", "A list of items to choose from.")] string choices = "Heads Tails")
        {
            MatchCollection captures = RegexArgumentParser.Matches(choices);
            return context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
            {
                Content = captures[Random.Shared.Next(0, captures.Count)].Value
            });
        }

        [GeneratedRegex("\"([^\"]+)\"|(\\S+)")]
        private static partial Regex RegexArgumentParserMethod();
    }
}
