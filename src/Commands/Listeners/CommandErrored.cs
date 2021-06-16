namespace Tomoe.Commands.Listeners
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using DSharpPlus.SlashCommands.EventArgs;
    using Humanizer;
    using System.Threading.Tasks;

    public class CommandErrored
    {
        public static async Task Handler(SlashCommandsExtension slashCommandExtension, SlashCommandErrorEventArgs slashCommandErrorEventArgs)
        {
            DiscordChannel discordChannel = await slashCommandExtension.Client.GetChannelAsync(832374606748188743);
            DiscordMessageBuilder discordMessageBuilder = new();
            discordMessageBuilder.Content = $"`/{slashCommandErrorEventArgs.Context.CommandName}` threw a {slashCommandErrorEventArgs.Exception.GetType()}: {slashCommandErrorEventArgs.Exception.Message ?? "<no message>"}\n{Formatter.BlockCode(slashCommandErrorEventArgs.Exception.StackTrace.Truncate(1800))}";
            await discordChannel.SendMessageAsync(discordMessageBuilder);
        }
    }
}
