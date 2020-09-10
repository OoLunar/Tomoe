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
        public async Task ByMention(SocketGuildUser muteMember, [Remainder] string reason = null) {
            //Get mute role
            ulong muteRole = Tomoe.Utils.Cache.MutedRole.Get(Context.Guild.Id).RoleID;

            //Add dialog context.
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Mute;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = muteMember;
            dialogContext.Reason = reason;

            //Check if user is still in the guild.
            if (muteMember == null || Context.Guild.GetUser(muteMember.Id) == null) {
                dialogContext.Error = Program.Dialogs.Message.Errors.NotInGuild;
                await dialogContext.SendChannel();
                return;
            }
            //Check if bot can give the mute role.
            else if (muteMember.Hierarchy >= Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedHierarchyError;
                await dialogContext.SendChannel();
                return;
            }
            // Check for bot self mute.
            else if (muteMember.Id == Program.Client.CurrentUser.Id) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedSelfBot;
                await dialogContext.SendChannel();
                return;
            }

            //TODO: Store old roles and give them back upon unmuting.
            muteMember.RemoveRolesAsync(muteMember.Roles);
            muteMember.AddRoleAsync(Context.Guild.GetRole(muteRole));

            // Create the DM Channel and tell them what happened.
            try {
                dialogContext.SendDM();
            } catch {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            }

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>
        /// Mutes a guild member identified by an ID with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.ManageRoles"/>` (bot) and `<see cref="Discord.GuildPermission.ManageMessages"/>` (user).
        /// <code>
        /// >>mute 336733686529654798 shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task ByID(ulong muteMember, [Remainder] string reason = null) => await ByMention(Context.Guild.GetUser(muteMember), reason);

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