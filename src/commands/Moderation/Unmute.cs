using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation {
    public class Unmute : BaseCommandModule {
        [Command("unmute")]
        [Description("Unmutes an individual.")]
        public async Task Individual(CommandContext context, DiscordUser victim, [RemainingText] string unmuteReason = Program.MissingReason) {
            if (victim == context.Guild.CurrentMember) {
                Program.SendMessage(context, Program.SelfAction);
                return;
            }
            ulong? muteRoleId = Program.Database.Guild.MuteRole(context.Guild.Id);
            if (!muteRoleId.HasValue) Program.SendMessage(context, Program.MissingMuteRole);
            else {
                DiscordRole muteRole = context.Guild.GetRole(muteRoleId.Value);
                if (muteRole == null) {
                    Program.SendMessage(context, Program.MissingMuteRole);
                    return;
                }
                DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
                try {
                    (await guildVictim.CreateDmChannelAsync()).SendMessageAsync($"You've been unmuted by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{unmuteReason.Filter(context)}\n```");
                } catch (NotFoundException) {
                    Program.SendMessage(context, $"Failed to message {victim.Mention}, but they have been unmuted. Reason: ```\n{unmuteReason.Filter(context, ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace)}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new System.Collections.Generic.List<IMention>() { new UserMention(victim.Id) });
                }
                await guildVictim.RevokeRoleAsync(muteRole, unmuteReason);
                Program.Database.User.IsMuted(context.Guild.Id, victim.Id, false);
                Program.SendMessage(context, $"{victim.Mention} has been unmuted. Reason: ```\n{unmuteReason.Filter(context)}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new System.Collections.Generic.List<IMention>() { new UserMention(victim.Id) });
            }
        }
    }
}