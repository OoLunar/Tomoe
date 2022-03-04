using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation
{
    public class Ban : BaseCommandModule
    {
        public ILogger<Ban> Logger { private get; init; } = null!;

        [RequireGuild]
        [Command("ban")]
        [Description("Bans a user from the server.")]
        public async Task BanAsync(CommandContext context, [Description("Who's getting banned?")] DiscordMember offender, [Description("Why are they getting banned?"), RemainingText] string reason = Constants.NoReasonSpecified) => await BanAsync(context, offender, 1, reason);

        [RequireGuild]
        [Command("ban")]
        [Description("Bans a user from the server.")]
        public async Task BanAsync(CommandContext context, [Description("Who's getting banned?")] DiscordMember offender, [Description("Delete their messages in the past X days (0-7)")] int deleteDays = 1, [Description("Why are they getting banned?"), RemainingText] string reason = Constants.NoReasonSpecified)
        {
            // Check if the user is already banned.
            if ((await context.Guild.GetBansAsync()).Any(guildUser => guildUser.User.Id == offender.Id))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: {offender.Mention} ({offender.Id}) is already banned!"));
                return;
            }

            // Check if the executing user can ban the offender.
            if (!context.Member.CanExecute(Permissions.BanMembers, offender))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: You cannot ban {offender.Mention} due to Discord permissions!"));
                return;
            }
            // Check if the bot can ban the offender.
            else if (!context.Guild.CurrentMember.CanExecute(Permissions.BanMembers, offender))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: I cannot ban {offender.Mention} due to Discord permissions!"));
                return;
            }

            // Attempt to Dm the user.
            bool dmSuccess = true;
            try
            {
                DiscordDmChannel dmChannel = await offender.CreateDmChannelAsync();
                await dmChannel.SendMessageAsync($"You have been banned from {context.Guild.Name} ({context.Guild.Id}) by {context.Member.Username}#{context.Member.Discriminator} ({context.Member.Id}) for the following reason:\n{reason}");
            }
            catch (Exception)
            {
                dmSuccess = false;
            }

            // Attempt to ban the user.
            try
            {
                await context.Guild.BanMemberAsync(offender, deleteDays, reason);
            }
            catch (Exception error)
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: Failed to ban {offender.Mention}: {error.Message}"));
                Logger.LogWarning("Uncaught exception: {@Exception}", error);
            }

            await context.RespondAsync($"{offender.Mention} ({offender.Id}) has been banned{(dmSuccess ? null : " (Failed to DM)")}.");

            // TODO: Modlog
        }

        [RequireGuild]
        [Command("ban")]
        [Description("Bans a user from the server.")]
        public async Task BanAsync(CommandContext context, [Description("Who's getting banned?")] DiscordUser offender, [Description("Why are they getting banned?"), RemainingText] string reason = Constants.NoReasonSpecified) => await BanAsync(context, offender, 1, reason);

        [RequireGuild]
        [Command("ban")]
        [Description("Bans a user from the server.")]
        public async Task BanAsync(CommandContext context, [Description("Who's getting banned?")] DiscordUser offender, [Description("Delete their messages in the past X days (0-7)")] int deleteDays = 1, [Description("Why are they getting banned?"), RemainingText] string reason = Constants.NoReasonSpecified)
        {
            // Check if the user is already banned.
            if ((await context.Guild.GetBansAsync()).Any(guildUser => guildUser.User.Id == offender.Id))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: {offender.Mention} ({offender.Id}) is already banned!"));
                return;
            }
            // Make sure the executing user can ban members.
            else if (!context.Member.Permissions.HasPermission(Permissions.BanMembers))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: You cannot ban {offender.Mention} due to Discord permissions!"));
                return;
            }
            // Make sure the bot can ban members.
            else if (!context.Guild.CurrentMember.Permissions.HasPermission(Permissions.BanMembers))
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: I cannot ban {offender.Mention} due to Discord permissions!"));
                return;
            }

            // Attempt to ban the user.
            try
            {
                await context.Guild.BanMemberAsync(offender.Id, deleteDays, reason);
            }
            catch (Exception error)
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: Failed to ban {offender.Mention}: {error.Message}"));
                Logger.LogWarning("Uncaught exception: {@Exception}", error);
            }

            await context.RespondAsync($"{offender.Mention} ({offender.Id}) has been banned (Failed to DM).");

            // TODO: Modlog
        }
    }
}