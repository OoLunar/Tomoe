using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed class Mute : BaseCommandModule
    {
        [Command("mute")]
        [Description("Mutes a user.")]
        [RequireGuild, RequirePermissions(Permissions.ModerateMembers)]
        public async Task MuteAsync(CommandContext context, DiscordMember member, TimeSpan? timeSpan = null, [RemainingText] string? reason = null)
        {
            if (member.CommunicationDisabledUntil != null)
            {
                await context.RespondAsync($"They're already muted until {Formatter.Timestamp(member.CommunicationDisabledUntil.Value, TimestampFormat.ShortDateTime)}");
                return;
            }
            else if (context.Member!.Hierarchy <= member.Hierarchy)
            {
                await context.RespondAsync("You cannot mute a user with a higher or equal role than you.");
                return;
            }
            else if (context.Guild.CurrentMember.Hierarchy <= member.Hierarchy)
            {
                await context.RespondAsync("I cannot mute a user with a higher or equal role than me.");
                return;
            }

            timeSpan ??= TimeSpan.FromMinutes(5);
            await member.TimeoutAsync(DateTimeOffset.UtcNow.Add(timeSpan.Value), $"Muted by {context.Member!.Username}#{context.Member.Discriminator}: {reason}");
            await context.RespondAsync($"{member.Username}#{member.Discriminator} has been muted for {Formatter.Timestamp(timeSpan.Value, TimestampFormat.ShortDateTime)}.");
        }
    }
}
