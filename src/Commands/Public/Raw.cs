namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Globalization;
    using System.Threading.Tasks;

    public class Raw : SlashCommandModule
    {
        [SlashCommand("raw", "Gets the raw version of the message provided.")]
        public async Task Overload(InteractionContext context, [Option("Message", "The message id or link.")] string messageString)
        {
            string rawText = await Api.Public.Raw(context, messageString);
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
            {
                Content = rawText,
                IsEphemeral = rawText.StartsWith("error:", true, CultureInfo.InvariantCulture)
            });
        }
    }
}
