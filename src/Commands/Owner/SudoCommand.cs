using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace OoLunar.Tomoe.Commands.Owner
{
    /// <summary>
    /// 'tomoe' is not in the sudoers file. This incident will be reported.
    /// </summary>
    public static class SudoCommand
    {
        /// <summary>
        /// Run a command as another user.
        /// </summary>
        [Command("sudo"), RequireApplicationOwner]
        public static async ValueTask ExecuteAsync(CommandContext context, DiscordMember member, [RemainingText] string command)
        {
            if (context is not TextCommandContext textCommandContext)
            {
                await context.RespondAsync("This command can only be used through text commands.");
                return;
            }

            // Change the user who's executing the command to the specified user
            textCommandContext = textCommandContext with
            {
                User = member
            };

            MessageCreatedEventArgs eventArgs = await TextCommandUtilities.CreateFakeMessageEventArgsAsync(textCommandContext, textCommandContext.Message, $"{context.Client.CurrentUser.Mention} {command}");
            await context.Extension.GetProcessor<TextCommandProcessor>().ExecuteTextCommandAsync(context.Client, eventArgs);
        }
    }
}
