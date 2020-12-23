using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Tomoe.Database.Interfaces;

namespace Tomoe.Commands.Moderation {
    [Group("strike")]
    [Description("Gives a strike/warning to the specified victim.")]
    [RequireUserPermissions(Permissions.KickMembers)]
    public class Strikes : BaseCommandModule {
        [GroupCommand]
        public async Task Add(CommandContext context, DiscordUser victim, [RemainingText] string strikeReason = Program.MissingReason) {
            if (victim == context.Guild.CurrentMember) {
                Program.SendMessage(context, Program.SelfAction);
                return;
            }

            bool sentDm = true;

            try {
                DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
                if (guildVictim.Hierarchy > (await context.Guild.GetMemberAsync(context.Client.CurrentUser.Id)).Hierarchy) {
                    Program.SendMessage(context, Program.Hierarchy);
                    return;
                }

                (await guildVictim.CreateDmChannelAsync()).SendMessageAsync($"You've been given a strike by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{strikeReason.Filter(context) ?? Program.MissingReason}\n```");
            } catch (NotFoundException) {
                sentDm = false;
            } catch (BadRequestException) {
                sentDm = false;
            }
            Strike strike = Program.Database.Strikes.Add(context.Guild.Id, victim.Id, context.User.Id, strikeReason, context.Message.JumpLink.ToString(), sentDm);
            Program.SendMessage(context, $"Case #{strike.Id}, {victim.Mention} has been striked{(sentDm == true ? '.' : " (Failed to DM).")} This is strike #{strike.StrikeCount}. Reason: ```\n{strikeReason.Filter(context, ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace) ?? Program.MissingReason}\n```", default, new List<IMention>() { new UserMention(victim.Id) });
        }

        [Command("check")]
        [Description("Gets the users past history")]
        [RequireUserPermissions(Permissions.KickMembers)]
        [Aliases("history")]
        public async Task Check(CommandContext context, DiscordUser victim) {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.Title = $"{victim.Username}'s Past History";
            Strike[] pastStrikes = Program.Database.Strikes.GetVictim(context.Guild.Id, victim.Id);
            if (pastStrikes == null) embedBuilder.Description = "No strikes have been found!";
            else
                foreach (Strike strike in Program.Database.Strikes.GetVictim(context.Guild.Id, victim.Id)) embedBuilder.Description += $"Case #{strike.Id} [on {strike.CreatedAt.ToString("MMM' 'dd', 'yyyy' 'HH':'mm':'ss")}, Issued by {(await context.Client.GetUserAsync(strike.IssuerId)).Mention}]({strike.JumpLink}) {(strike.Dropped ? "(Dropped)" : null)}\n";

            Program.SendMessage(context, embedBuilder.Build());
        }
    }
}