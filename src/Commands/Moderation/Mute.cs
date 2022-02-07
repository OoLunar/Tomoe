using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Tomoe.Commands.Moderation
{
    public class Mute : BaseCommandModule
    {
        public ILogger<Mute> Logger { private get; init; } = null!;

        [RequireGuild]
        [Command("mute")]
        [Description("Using the Discord Timeout feature, the user is unable to communicate with their guild members.")]
        public async Task MuteUserAsync(CommandContext context, [Description("Who's getting muted?")] DiscordMember offender, [Description("How long will they be muted?")] TimeSpan timeSpan, [Description("Why are they getting muted?"), RemainingText] string reason = Constants.NoReasonSpecified)
        {
            // Check if the executing user can mute members.
            if (!context.Member.Permissions.HasPermission(Permissions.ModerateMembers))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: You cannot mute {offender.Mention} due to Discord permissions!"));
                return;
            }
            // Check if the bot can mute members.
            else if (!context.Guild.CurrentMember.Permissions.HasPermission(Permissions.ModerateMembers))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: I cannot mute {offender.Mention} due to Discord permissions!"));
                return;
            }

            // Attempt to Dm the user.
            bool dmSuccess = true;
            try
            {
                DiscordDmChannel dmChannel = await offender.CreateDmChannelAsync();
                await dmChannel.SendMessageAsync($"You have been muted from {context.Guild.Name} ({context.Guild.Id}) by {context.Member.Username}#{context.Member.Discriminator} ({context.Member.Id}) for the following reason:\n{reason}");
            }
            catch (Exception)
            {
                dmSuccess = false;
            }

            // Attempt to mute the user.
            try
            {
                await offender.TimeoutAsync(DateTime.UtcNow.Add(timeSpan), reason);
            }
            catch (Exception error)
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: Failed to mute {offender.Mention}: {error.Message}"));
                Logger.LogWarning("Uncaught exception: {@Exception}", error);
                return;
            }

            await context.RespondAsync(Formatter.Bold($"{offender.Mention} ({offender.Id}) has been muted{(dmSuccess ? null : " (Failed to DM)")}! The mute will be removed {Formatter.Timestamp(timeSpan)}!"));

            // TODO: Modlog
        }
    }
}