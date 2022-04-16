using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace Tomoe.Commands.Moderation
{
    public class Unban : BaseCommandModule
    {
        public ILogger<Unban> Logger { private get; init; } = null!;

        [Command("unban"), Description("Unbans a user from the server."), RequireGuild, RequirePermissions(Permissions.BanMembers)]
        public async Task UnbanAsync(CommandContext context, [Description("Who's getting unbanned?")] DiscordUser offender, [Description("Why are they getting unbanned?"), RemainingText] string reason = Constants.NoReasonSpecified)
        {
            // Check if the user is already banned.
            if ((await context.Guild.GetBanAsync(offender.Id)) == null)
            {
                await context.RespondAsync($"[Error]: {offender.Mention} ({offender.Id}) is not banned!");
                return;
            }

            // Attempt to unban the user.
            try
            {
                await context.Guild.UnbanMemberAsync(offender, reason);
            }
            catch (DiscordException error)
            {
                await context.RespondAsync($"[Error]: Failed to unban {offender.Mention}. Error: (HTTP {error.WebResponse.ResponseCode}) {error.JsonMessage}");
                Logger.LogWarning(error, "Failed to unban {Offender} from guild {GuildId}. Error: (HTTP {HTTPCode}) {JsonError}", offender.Id, context.Guild.Id, error.WebResponse.ResponseCode, error.JsonMessage);
                return;
            }

            await context.RespondAsync(Formatter.Bold($"{offender.Mention} ({offender.Id}) has been unbanned (Failed to DM)!"));
        }
    }
}
