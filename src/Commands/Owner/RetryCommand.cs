using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace OoLunar.Tomoe.Commands.Owner
{
    /// <summary>
    /// Pete and Repeat were on a boat...
    /// </summary>
    public static class RetryCommand
    {
        /// <summary>
        /// Attempts to run the passed Discord message as a command.
        /// </summary>
        /// <param name="message">The message to re-run.</param>
        [Command("retry"), TextAlias("rerun"), RequireApplicationOwner]
        public static async ValueTask RetryAsync(CommandContext context, DiscordMessage message)
        {
            if (context is not TextCommandContext textCommandContext)
            {
                await context.RespondAsync("This command can only be used through text commands.");
                return;
            }

            MessageCreatedEventArgs eventArgs = await TextCommandUtilities.CreateFakeMessageEventArgsAsync(textCommandContext, message, message.Content);
            await context.Extension.GetProcessor<TextCommandProcessor>().ExecuteTextCommandAsync(context.Client, eventArgs);
        }
    }
}
