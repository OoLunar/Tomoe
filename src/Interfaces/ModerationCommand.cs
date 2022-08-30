using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using OoLunar.Tomoe.Utilities;

namespace OoLunar.Tomoe.Interfaces
{
    public abstract class ModerationCommand : AuditableCommand
    {
        public override async Task BeforeExecutionAsync(CommandContext context)
        {
            if (context.Guild == null)
            {
                throw new InvalidOperationException("This command can only be used in a guild.");
            }

            await base.BeforeExecutionAsync(context);
        }

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
