namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using DSharpPlus.SlashCommands.EventArgs;
    using Humanizer;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class Listeners
    {
        public static async Task CommandErrored(SlashCommandsExtension slashCommandExtension, SlashCommandErrorEventArgs slashCommandErrorEventArgs)
        {
            if (slashCommandErrorEventArgs.Exception is SlashExecutionChecksFailedException)
            {
                return;
            }
#if DEBUG
            else if (slashCommandErrorEventArgs.Exception is SlashExecutionChecksFailedException && slashCommandErrorEventArgs.Exception.StackTrace.Contains("CreateInteractionResponseAsync"))
            {
                return;
            }
#endif
            DiscordChannel discordChannel = await slashCommandExtension.Client.GetChannelAsync(832374606748188743);
            DiscordMessageBuilder discordMessageBuilder = new();
            string stackTrace = string.Join("\n\n", slashCommandErrorEventArgs.Exception.StackTrace.Split('\n').Select(line => line.Trim())).Truncate(1800);
            discordMessageBuilder.Content = $"`/{slashCommandErrorEventArgs.Context.CommandName}` threw a {slashCommandErrorEventArgs.Exception.GetType()}: {slashCommandErrorEventArgs.Exception.Message ?? "<no message>"}\n{Formatter.BlockCode(stackTrace, "cs")}";
            await discordChannel.SendMessageAsync(discordMessageBuilder);
        }
    }
}
