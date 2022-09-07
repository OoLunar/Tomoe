using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed class UnbanCommand : ModerationCommand
    {
        [Command("unban")]
        [Description("Unbans a user from the server.")]
        [RequireGuild, RequirePermissions(Permissions.BanMembers)]
        public async Task UnbanAsync(CommandContext context, [Description("The user to unban.")] DiscordUser user, [RemainingText] string? reason = null)
        {
            DiscordBan? ban = null;
            if (user is DiscordMember || (ban = await context.Guild.GetBanAsync(user)) == null)
            {
                await context.RespondAsync("They're not banned!");
                Audit.AddNote("User wasn't banned.");
                return;
            }

            Audit.AffectedUsers = new[] { user.Id };
            reason = Audit.SetReason(reason);
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

            await context.RespondAsync($"Unbanned {user.Username}#{user.Discriminator}. {Audit.Notes![0]}");
        }
    }
}
