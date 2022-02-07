using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Tomoe.Commands.Moderation
{
    public class Unban : BaseCommandModule
    {
        public ILogger<Unban> Logger { private get; init; } = null!;

        [RequireGuild]
        [Command("unban")]
        [Description("Unbans a user from the server.")]
        public async Task UnbanAsync(CommandContext context, [Description("Who's getting unbanned?")] DiscordUser offender, [Description("Why are they getting unbanned?"), RemainingText] string reason = Constants.NoReasonSpecified)
        {
            // Check if the user is already banned.
            if (!(await context.Guild.GetBansAsync()).Any(guildUser => guildUser.User.Id == offender.Id))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: {offender.Mention} ({offender.Id}) is not banned!"));
                return;
            }

            // Check if the executing user can unban members
            if (!context.Member.Permissions.HasPermission(Permissions.BanMembers))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: You cannot unban {offender.Mention} due to Discord permissions!"));
                return;
            }
            // Check if the bot can unban members
            else if (!context.Guild.CurrentMember.Permissions.HasPermission(Permissions.BanMembers))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: I cannot unban {offender.Mention} due to Discord permissions!"));
                return;
            }

            // Attempt to unban the user.
            try
            {
                await context.Guild.UnbanMemberAsync(offender, reason);
            }
            catch (Exception error)
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: Failed to unban {offender.Mention}: {error.Message}"));
                Logger.LogWarning("Uncaught exception: {@Exception}", error);
                return;
            }

            await context.RespondAsync(Formatter.Bold($"{offender.Mention} ({offender.Id}) has been unbanned (Failed to DM)!"));
        }
    }
}