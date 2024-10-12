using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Pete and Repeat were on a boat...
    /// </summary>
    public static class RetryCommand
    {
        /// <summary>
        /// Attempts to run the passed Discord message as a command.
        /// </summary>
        /// <remarks>
        /// Only the bot owner can rerun someone else's command, or run a command as someone else.
        /// </remarks>
        /// <param name="message">The message to re-run.</param>
        /// <param name="asWho">Who to run the command as.</param>
        [Command("retry"), TextAlias("rerun")]
        public static async ValueTask RetryAsync(CommandContext context, DiscordMessage message, DiscordUser? asWho = null)
        {
            if (context is not TextCommandContext textCommandContext)
            {
                await context.RespondAsync("This command can only be used through text commands.");
                return;
            }
            // We use `is not true` since it could be null or false
            else if (context.Client.CurrentApplication.Owners?.Contains(context.User) is not true
                // If the message belongs to someone else or if we're changing the message to be sent as someone else
                && (message.Author != context.User || (asWho is not null && asWho != context.User)))
            {
                await context.RespondAsync("You must be the bot owner to run this command as someone else.");
            }
            else if (asWho is not null)
            {
                await TextCommandUtilities.ModifyMessagePropertiesAsync(message, message.Content, context.Client, asWho, message.Channel!, message.Channel?.Guild);
            }

            MessageCreatedEventArgs eventArgs = await TextCommandUtilities.CreateFakeMessageEventArgsAsync(textCommandContext, message, message.Content);
            await context.Extension.GetProcessor<TextCommandProcessor>().ExecuteTextCommandAsync(context.Client, eventArgs);
        }
    }
}
