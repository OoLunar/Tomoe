using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace Tomoe.Commands.Moderation
{
    public class Unmute : BaseCommandModule
    {
        public ILogger<Unmute> Logger { private get; init; } = null!;

        [Command("unmute"), Description("Unmutes a member from the server."), RequireGuild, RequirePermissions(Permissions.ModerateMembers)]
        public async Task UnmuteAsync(CommandContext context, [Description("Who's getting unmuted?")] DiscordMember offender, [Description("Why are they getting unmuted?"), RemainingText] string reason = Constants.NoReasonSpecified)
        {
            // Attempt to Dm the user.
            bool dmSuccess = true;
            try
            {
                DiscordDmChannel dmChannel = await offender.CreateDmChannelAsync();
                await dmChannel.SendMessageAsync($"You have been unmuted from {context.Guild.Name} ({context.Guild.Id}) by {context.Member!.Username}#{context.Member.Discriminator} ({context.Member.Id}) for the following reason:\n{reason}");
            }
            catch (DiscordException)
            {
                dmSuccess = false;
            }

            // Attempt to unmute the user.
            try
            {
                await offender.TimeoutAsync(null, reason);
            }
            catch (DiscordException error)
            {
                await context.RespondAsync($"[Error]: Failed to unmute {offender.Mention}. Error: (HTTP {error.WebResponse.ResponseCode}) {error.JsonMessage}");
                Logger.LogWarning(error, "Failed to unmute {Offender} from guild {GuildId}. Error: (HTTP {HTTPCode}) {JsonError}", offender.Id, context.Guild.Id, error.WebResponse.ResponseCode, error.JsonMessage);
                return;
            }

            await context.RespondAsync(Formatter.Bold($"{offender.Mention} ({offender.Id}) has been unmuted{(dmSuccess ? null : " (Failed to DM)")}!"));
        }
    }
}
