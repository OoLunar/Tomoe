using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Unbans a user from the guild.
    /// </summary>
    public sealed class UnbanCommand : ModerationCommand
    {
        /// <summary>
        /// Unbans a user from the guild.
        /// </summary>
        /// <param name="context">The context that the command was used in.</param>
        /// <param name="user">The user to unban.</param>
        /// <param name="reason">Why the user is being unbanned.</param>
        [Command("unban")]
        [Description("Unbans a user from the guild.")]
        [RequireGuild, RequirePermissions(Permissions.BanMembers)]
        public async Task UnbanAsync(CommandContext context, [Description("The user to unban.")] DiscordUser user, [Description("Why the user is being unbanned."), RemainingText] string? reason = null)
        {
            // Declare for scope.
            DiscordBan? ban;
            // We don't have to do permission checks due to RequirePermissions checking both the bot and the executor.
            // If the user is a DiscordMember, that means they're still in the guild. Otherwise, check with the Discord API to see if they're banned.
            if (user is DiscordMember || (ban = await context.Guild.GetBanAsync(user)) == null)
            {
                await context.RespondAsync("They're not banned!");
                Audit.AddNote("User wasn't banned.");
                return;
            }

            Audit.AddAffectedUsers(user.Id);
            reason = Audit.SetReason(reason);
            // Make it easier to figure out the ban history.
            Audit.AddNote($"They were previously banned by {ban.User.Username}#{ban.User.Discriminator} for: {ban.Reason}.");

            try
            {
                await context.Guild.UnbanMemberAsync(user, reason);
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to unban, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                await context.RespondAsync($"I was unable to unban the user, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                Audit.Successful = false;
                return;
            }

            // We safely index the notes property to either inform the user for their previous ban reason or to inform them why the bot couldn't unban them.
            await context.RespondAsync($"Unbanned {user.Username}#{user.Discriminator}. {Audit.Notes[0]}");
        }
    }
}
