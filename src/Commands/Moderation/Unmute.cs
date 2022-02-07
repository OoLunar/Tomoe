using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Tomoe.Commands.Moderation
{
    public class Unmute : BaseCommandModule
    {
        public ILogger<Unmute> Logger { private get; init; } = null!;

        [RequireGuild]
        [Command("unmute")]
        [Description("Unmutes a user from the server.")]
        public async Task UnmuteAsync(CommandContext context, [Description("Who's getting unmuted?")] DiscordMember offender, [Description("Why are they getting unmuted?"), RemainingText] string reason = Constants.NoReasonSpecified)
        {
            // Check if the executing user can unmute members.
            if (!context.Member.Permissions.HasPermission(Permissions.ModerateMembers))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: You cannot unmute {offender.Mention} due to Discord permissions!"));
                return;
            }
            // Check if the bot can unmute members.
            else if (!context.Guild.CurrentMember.Permissions.HasPermission(Permissions.ModerateMembers))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: I cannot unmute {offender.Mention} due to Discord permissions!"));
                return;
            }

            // Attempt to Dm the user.
            bool dmSuccess = true;
            try
            {
                DiscordDmChannel dmChannel = await offender.CreateDmChannelAsync();
                await dmChannel.SendMessageAsync($"You have been unmuted from {context.Guild.Name} ({context.Guild.Id}) by {context.Member.Username}#{context.Member.Discriminator} ({context.Member.Id}) for the following reason:\n{reason}");
            }
            catch (Exception)
            {
                dmSuccess = false;
            }

            // Attempt to unmute the user.
            try
            {
                await offender.TimeoutAsync(null, reason);
            }
            catch (Exception error)
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: Failed to unmute {offender.Mention}: {error.Message}"));
                Logger.LogWarning("Uncaught exception: {@error}", error);
                return;
            }

            await context.RespondAsync(Formatter.Bold($"{offender.Mention} ({offender.Id}) has been unmuted{(dmSuccess ? null : " (Failed to DM)")}!"));

            // TODO: Modlog
        }
    }
}