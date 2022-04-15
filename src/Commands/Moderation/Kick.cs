using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation
{
    public class Kick : BaseCommandModule
    {
        public ILogger<Kick> Logger { private get; init; } = null!;

        [RequireGuild]
        [Command("kick")]
        [Description("Kicks a user from the server.")]
        public async Task KickAsync(CommandContext context, [Description("Who's getting kicked?")] DiscordMember offender, [Description("Why are they getting kicked?"), RemainingText] string reason = Constants.NoReasonSpecified)
        {
            // Check if the executing user can kick members.
            if (!context.Member.CanExecute(Permissions.KickMembers, offender))
            {
                await context.RespondAsync($"[Error]: You cannot kick {offender.Mention} due to Discord permissions!");
                return;
            }
            // Check if the bot can kick members.
            else if (!context.Guild.CurrentMember.CanExecute(Permissions.KickMembers, offender))
            {
                await context.RespondAsync($"[Error]: I cannot kick {offender.Mention} due to Discord permissions!");
                return;
            }

            // Attempt to Dm the user.
            bool dmSuccess = true;
            try
            {
                DiscordDmChannel dmChannel = await offender.CreateDmChannelAsync();
                await dmChannel.SendMessageAsync($"You have been kicked from {context.Guild.Name} ({context.Guild.Id}) by {context.Member.Username}#{context.Member.Discriminator} ({context.Member.Id}) for the following reason:\n{reason}");
            }
            catch (DiscordException)
            {
                dmSuccess = false;
            }

            // Attempt to kick the user.
            try
            {
                await offender.RemoveAsync(reason);
            }
            catch (DiscordException error)
            {
                await context.RespondAsync($"[Error]: Failed to kick {offender.Mention}. Error: (HTTP {error.WebResponse.ResponseCode}) {error.JsonMessage}");
                Logger.LogWarning(error, "Failed to kick {Offender} from guild {GuildId}. Error: (HTTP {HTTPCode}) {JsonError}", offender.Id, context.Guild.Id, error.WebResponse.ResponseCode, error.JsonMessage);
                return;
            }

            await context.RespondAsync(Formatter.Bold($"{offender.Mention} ({offender.Id}) has been kicked{(dmSuccess ? null : " (Failed to DM)")}!"));

            // TODO: Modlog
        }
    }
}
