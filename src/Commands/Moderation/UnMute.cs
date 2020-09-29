using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils.Dialog;

namespace Tomoe.Commands.Moderation {
    public class UnMute : InteractiveBase {

        [Command("unmute", RunMode = RunMode.Async)]
        [Summary("Unmutes a member.")]
        [Remarks("Moderation")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByID(ulong victimId, [Remainder] string reason = null) {
            Context dialogContext = new Context();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = await Program.Client.Rest.GetUserAsync(victimId);
            dialogContext.UserAction = Tomoe.Utils.Dialog.Context.Action.UnMute;
            dialogContext.RequiredGuildPermission = GuildPermission.ManageMessages;
            dialogContext.Reason = reason;

            // Get mute role
            Utils.Cache.MutedRole? muteRole = Tomoe.Utils.Cache.MutedRole.Get(Context.Guild.Id);

            ulong muteRoleId = muteRole.RoleID;
            SocketGuildUser muteMember = Context.Guild.GetUser(victimId);

            // Victim does not exist.
            if (dialogContext.Victim == null) return;
            // Victim is not in the guild.
            else if (muteMember == null) {
                dialogContext.Error = Program.Dialogs.Message.Errors.NotInGuild;
                dialogContext.SendChannel();
                return;
            }
            // Check for bot self mute.
            else if (dialogContext.Victim.Id == Program.Client.CurrentUser.Id) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedSelfBot;
                await dialogContext.SendChannel();
                return;
            }
            // Check if bot can mute user.
            else if (muteMember != null && muteMember.Hierarchy >= Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedHierarchyError;
                await dialogContext.SendChannel();
                return;
            }
            // If the user is not marked as muted or does not have the muted role
            else if (!Utils.Cache.MutedRole.GetMute(muteMember.Id, Context.Guild.Id).Value || !(muteMember.Roles as System.Collections.Generic.List<SocketRole>).Contains(Context.Guild.GetRole(muteRole.RoleID))) {
                dialogContext.Error = Program.Dialogs.Message.Errors.UserNotMuted;
                await dialogContext.SendChannel();
                return;
            }

            try {
                // Give the users roles back.
                muteMember.RemoveRoleAsync(Context.Guild.GetRole(muteRole.RoleID));
                ulong[] roles = Tomoe.Utils.Cache.UserRoles.Get(Context.Guild.Id, muteMember.Id);
                if (roles != null)
                    foreach (ulong roleID in roles) muteMember.AddRoleAsync(Context.Guild.GetRole(roleID));

                // Let the victim know they were unmuted.
                await dialogContext.SendDM();
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 50007) {
                // If the victim cannot be DM'd, let the issuer know.
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            }

            dialogContext.SendChannel();
            return;
        }

        [Command("unmute", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByMention(SocketGuildUser victim, [Remainder] string reason = null) => await ByID(victim.Id, reason);

        public static async Task ByProgram(ulong guildId, ulong victimId, string reason = null) {
            Context dialogContext = new Context();
            dialogContext.Guild = Program.Client.GetGuild(guildId);
            dialogContext.Issuer = Program.Client.CurrentUser;
            dialogContext.Victim = await Program.Client.Rest.GetUserAsync(victimId);
            dialogContext.UserAction = Tomoe.Utils.Dialog.Context.Action.UnMute;
            dialogContext.RequiredGuildPermission = GuildPermission.ManageMessages;
            dialogContext.Reason = reason;

            // Get mute role
            Utils.Cache.MutedRole? muteRole = Tomoe.Utils.Cache.MutedRole.Get(guildId);

            ulong muteRoleId = muteRole.RoleID;
            SocketGuildUser muteMember = await dialogContext.Guild.GetUserAsync(victimId) as SocketGuildUser;

            // Victim does not exist.
            // Victim is not in the guild.
            // Check for bot self mute.
            if (dialogContext.Victim == null || muteMember == null || dialogContext.Victim.Id == Program.Client.CurrentUser.Id) return;
            // Check if bot can mute user.
            else if (muteMember != null && muteMember.Hierarchy >= (await dialogContext.Guild.GetUserAsync(Program.Client.CurrentUser.Id) as SocketGuildUser).Hierarchy) return;
            // If the user is not marked as muted or does not have the muted role
            else if (!Utils.Cache.MutedRole.GetMute(muteMember.Id, guildId).Value || !(muteMember.Roles as System.Collections.Generic.List<SocketRole>).Contains(dialogContext.Guild.GetRole(muteRole.RoleID) as SocketRole)) return;

            try {
                // Give the users roles back.
                muteMember.RemoveRoleAsync(dialogContext.Guild.GetRole(muteRole.RoleID));
                ulong[] roles = Tomoe.Utils.Cache.UserRoles.Get(guildId, muteMember.Id);
                if (roles != null)
                    foreach (ulong roleID in roles) muteMember.AddRoleAsync(dialogContext.Guild.GetRole(roleID));

                // Let the victim know they were unmuted.
                await dialogContext.SendDM();
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 50007) {
                // If the victim cannot be DM'd, let the issuer know.
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            }
        }
    }
}