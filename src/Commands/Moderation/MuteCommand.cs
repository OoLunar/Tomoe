using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed class MuteCommand
    {
        [Command("mute"), RequirePermissions(Permissions.ModerateMembers), RequireGuild]
        public static async ValueTask MuteAsync(CommandContext context, DiscordMember? member = null, TimeSpan? timeSpan = null, [RemainingText] string? reason = null) => await ExecuteAsync(context, "Muted {0} for {1}. Reason: {2}", member, timeSpan, reason);

        [Command("sleep"), RequirePermissions(Permissions.ModerateMembers), RequireGuild]
        public static async ValueTask SleepAsync(CommandContext context, DiscordMember? member = null, TimeSpan? timeSpan = null) => await ExecuteAsync(context, "Go sleep {0}. I'll see you in {1}.", member, timeSpan, null);

        private static async ValueTask ExecuteAsync(CommandContext context, string muteText, DiscordMember? member = null, TimeSpan? timeSpan = null, [RemainingText] string? reason = null)
        {
            if (member is null)
            {
                if (context is not TextCommandContext textCommandContext || textCommandContext.Message.ReferencedMessage is null)
                {
                    await context.RespondAsync("Who am I supposed to mute? Is this some cruel joke?");
                    return;
                }

                member = await context.Guild!.GetMemberAsync(textCommandContext.Message.ReferencedMessage.Author!.Id);
            }

            reason ??= "None provided.";
            timeSpan ??= TimeSpan.FromMinutes(5);
            await member.TimeoutAsync(DateTimeOffset.UtcNow.Add(timeSpan.Value), reason);
            await context.RespondAsync(string.Format(CultureInfo.InvariantCulture, muteText, member.Mention, timeSpan.Value.Humanize(), reason));
        }
    }
}
