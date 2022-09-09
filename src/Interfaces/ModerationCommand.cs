using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using OoLunar.Tomoe.Utilities;

namespace OoLunar.Tomoe.Interfaces
{
    /// <summary>
    /// Used as a base for moderation commands. Provides helper methods.
    /// </summary>
    public abstract class ModerationCommand : AuditableCommand
    {
        /// <summary>
        /// Ensure that the command is only used in the guild and prepare the audit log.
        /// </summary>
        public override async Task BeforeExecutionAsync(CommandContext context)
        {
            if (context.Guild == null)
            {
                throw new InvalidOperationException("This command can only be used in a guild.");
            }

            await base.BeforeExecutionAsync(context);
        }

        /// <summary>
        /// Ensures that both the bot and the executing user have the specified permissions to preform X action on the member without receiving an error from the Discord API due to hierarchy or other external factors.
        /// </summary>
        /// <param name="context">The <see cref="CommandContext"/> context provided by CommandsNext.</param>
        /// <param name="permissions">The permissions to check for.</param>
        /// <param name="member">The member executing the command that requires the <paramref name="permissions"/> check.</param>
        /// <remarks>
        /// The method will use the <paramref name="context"/> to respond with an error message if the bot or the user does not have the required permissions. If this method returns <see langword="false"/>, the command should exit immediately.
        /// </remarks>
        /// <returns><see langword="true"/> if the action can be successfully executed, <see langword="false"/> otherwise.</returns>
        public async Task<bool> CheckPermissionsAsync(CommandContext context, Permissions permissions, DiscordMember member)
        {
            if (!context.Member!.CanExecute(permissions, member))
            {
                await context.RespondAsync("You cannot kick a user with a higher or equal role than you.");
                Audit.AddNote("User has a higher or equal role than the authorizer.");
                return false;
            }
            else if (!context.Guild.CurrentMember.CanExecute(permissions, member))
            {
                await context.RespondAsync("I cannot kick a user with a higher or equal role than me.");
                Audit.AddNote("User has a higher or equal role than me.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to DM a message to the specified <see cref="DiscordMember"/>.
        /// </summary>
        /// <param name="member">The member to DM.</param>
        /// <param name="message">The message to send.</param>
        /// <returns><see langword="true"/> if the message was sent successfully, <see langword="false"/> otherwise.</returns>
        public async Task<bool> DmMemberAsync(DiscordMember member, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));
            }

            try
            {
                await member.SendMessageAsync(message);
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to DM, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                return false;
            }
            return true;
        }
    }
}
