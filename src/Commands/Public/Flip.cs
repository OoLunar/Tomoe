namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class Flip : SlashCommandModule
    {
        [SlashCommand("flip", "Choose from the options you provide!")]
        public static async Task Overload(InteractionContext context, [Option("Choices", "A list of items to choose from.")] string choices = "Heads Tails") => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = Api.Public.Choose(choices)
        });
    }
}
