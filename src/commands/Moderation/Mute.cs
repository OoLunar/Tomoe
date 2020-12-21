using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation {
    public class Mute : BaseCommandModule {
        [Command("mute")]
        [Description("Mutes a person permanently.")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task MutePermanently(CommandContext context, DiscordUser victim, [RemainingText] string muteReason = Program.MissingReason) {
            if (victim == context.Client.CurrentUser) {
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
                    if (guildVictim.Hierarchy > (await context.Guild.GetMemberAsync(context.Client.CurrentUser.Id)).Hierarchy) {
                        Program.SendMessage(context, Program.Hierarchy);
                        return;
                    }
                    (await guildVictim.CreateDmChannelAsync()).SendMessageAsync($"You've been muted by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{muteReason.Filter(context)}\n```");
                } catch (NotFoundException) {
                    Program.SendMessage(context, $"Failed to message {victim.Mention}, but they have been muted. Reason: ```\n{muteReason.Filter(context, ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace)}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new System.Collections.Generic.List<IMention>() { new UserMention(victim.Id) });
                }
                await guildVictim.GrantRoleAsync(muteRole, muteReason);
                Program.Database.User.IsMuted(context.Guild.Id, victim.Id, true);
                Program.SendMessage(context, $"{victim.Mention} has been muted. Reason: ```\n{muteReason.Filter(context)}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new System.Collections.Generic.List<IMention>() { new UserMention(victim.Id) });
            }
        }
    }
}