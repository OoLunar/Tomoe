using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Humanizer;
using Serilog;

namespace Tomoe.Commands
{
    public sealed class CommandErroredListener
    {
        private static readonly ILogger Logger = Log.ForContext<CommandErroredListener>();

        public static async Task CommandErroredAsync(SlashCommandsExtension slashCommandExtension, SlashCommandErrorEventArgs slashCommandErrorEventArgs)
        {
            if (slashCommandErrorEventArgs.Exception is ApplicationCommandExecutionChecksFailedException)
            {
                return;
            }
            else if (slashCommandErrorEventArgs.Exception is InvalidOperationException && slashCommandErrorEventArgs.Exception.Message == "A slash command was executed, but no command was registered for it.")
            {
                return;
            }
            DiscordChannel discordChannel = await slashCommandExtension.Client.GetChannelAsync(832374606748188743);
            DiscordMessageBuilder discordMessageBuilder = new();
            string stackTrace = string.Join("\n\n", slashCommandErrorEventArgs.Exception.StackTrace?.Split('\n').Select(line => line.Trim()) ?? new[] { "No stack trace." }).Truncate(1800);
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"`/{slashCommandErrorEventArgs.Context.CommandName}` threw a {slashCommandErrorEventArgs.Exception.GetType()}: {slashCommandErrorEventArgs.Exception.Message ?? "<no message>"}");
            if (slashCommandErrorEventArgs.Exception.InnerException != null)
            {
                stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Inner Exception: {slashCommandErrorEventArgs.Exception.InnerException.GetType()}: {slashCommandErrorEventArgs.Exception.InnerException.Message ?? "<no message>"}");
            }
            stringBuilder.AppendLine(Formatter.BlockCode(stackTrace, "cs"));
            discordMessageBuilder.Content = stringBuilder.ToString();
            await discordChannel.SendMessageAsync(discordMessageBuilder);

            Logger.Error("{exception}", slashCommandErrorEventArgs.Exception);
        }
    }
}
