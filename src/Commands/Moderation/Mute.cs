using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation {
    public class HardMute : InteractiveBase {

        /// <summary>
        /// Mutes a guild member permanently, identified by a mention optionally with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.ManageRoles"/>` (bot) and `<see cref="Discord.GuildPermission.ManageMessages"/>` (user).
        /// <code>
        /// >>mute &lt;@336733686529654798&gt; shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        [Summary("[Mutes a member by mention or ID](https://github.com/OoLunar/Tomoe/tree/master/docs/moderation/mute.md)")]
        [Remarks("Moderation")]
        public async Task ByID(ulong victimId, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = await Program.Client.Rest.GetUserAsync(victimId);
            dialogContext.UserAction = DialogContext.Action.Mute;
            dialogContext.RequiredGuildPermission = GuildPermission.KickMembers;
            dialogContext.Reason = reason;

            // Get mute role
            Utils.Cache.MutedRole? muteRole = Tomoe.Utils.Cache.MutedRole.Get(Context.Guild.Id);
            if (muteRole == null) {
                // TODO: Edit dialog.jsonc and add this error.
                dialogContext.Error = "There is no mute role set.";
                dialogContext.SendChannel();
                return;
            }

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

            try {
                Utils.Cache.MutedRole.SetMute(muteMember.Id, Context.Guild.Id, true);
                // Store old roles and give them back upon unmuting.
                System.Collections.Generic.List<long> muteMembersRoles = new System.Collections.Generic.List<long>();
                foreach (SocketRole role in muteMember.Roles.ExceptEveryoneRole()) muteMembersRoles.Add((long) role.Id);
                Utils.Cache.UserRoles.Store(Context.Guild.Id, muteMember.Id, muteMembersRoles.ToArray());

                // Remove all roles, apply muted role.
                muteMember.RemoveRolesAsync(muteMember.Roles.ExceptEveryoneRole());
                muteMember.AddRoleAsync(Context.Guild.GetRole(muteRoleId));
                // Let the victim know they were muted.
                await dialogContext.SendDM();
                // Remove DM error.
                //TODO: Fix this. Check SendDM method.
                dialogContext.Error = null;
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 50007) {
                // If the victim cannot be DM'd, let the issuer know.
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            }

            dialogContext.SendChannel();
            return;
        }

        [Command("mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task ByMention(Mention victim, [Remainder] string reason = null) => ByID(victim.Id, reason);

        /// <summary>Informs the issuer that a mute could not be issued due to lack of bot/user permissions.</summary>
        [Command("mute", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task MuteNoPerms(SocketGuildUser muteMember, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Mute;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = muteMember;
            dialogContext.Reason = reason;

            if (!Context.Guild.GetUser(Program.Client.CurrentUser.Id).GuildPermissions.ManageRoles) {
                dialogContext.RequiredGuildPermission = GuildPermission.ManageRoles;
                dialogContext.Error = Program.Dialogs.Message.Errors.NoBotPerms;
            } else if (!Context.Guild.GetUser(Context.User.Id).GuildPermissions.ManageMessages) {
                dialogContext.RequiredGuildPermission = GuildPermission.ManageMessages;
                dialogContext.Error = Program.Dialogs.Message.Errors.NoUserPerms;
            }
            await dialogContext.SendChannel();
            return;
        }

        /// <summary>Informs the user that mutes cannot be issued in DMs.</summary>
        [Command("mute", RunMode = RunMode.Async)]
        [RequireContext(ContextType.DM)]
        public async Task DM(IUser muteMember, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Mute;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = muteMember;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>Informs the issuer that a mute cannot be issued in Group Chats.</summary>
        [Command("mute", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Group)]
        public async Task Group(IUser muteMember, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Mute;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = muteMember;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }
    }
}