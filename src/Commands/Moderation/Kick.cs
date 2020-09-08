using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation {
    public class Kick : InteractiveBase {

        /// <summary>
        /// Kicks a guild member identified by a mention, optionally with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.KickMembers"/>` (bot) and `<see cref="Discord.GuildPermission.KickMembers"/>` (user).
        /// <code>
        /// >>kick &lt;@336733686529654798&gt; shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("kick", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        [Summary("Kicks a guild member identified by a mention. Optionally with a reason.")]
        public async Task ByMention(SocketGuildUser kickMember, [Remainder] string reason = null) {
            //Add dialog context.
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Kick;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = kickMember;
            dialogContext.Reason = reason;

            //Check if user is in the guild.
            if (kickMember == null || Context.Guild.GetUser(kickMember.Id) == null) {
                dialogContext.Error = Program.Dialogs.Message.Errors.NotInGuild;
                await dialogContext.SendChannel();
                return;
            }
            // Check for bot self kick.
            else if (kickMember.Id == Program.Client.CurrentUser.Id) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedSelfBot;
                await dialogContext.SendChannel();
                return;
            }
            //Check if bot can kick user.
            else if (kickMember.Hierarchy >= Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedHierarchyError;
                await dialogContext.SendChannel();
                return;
            }

            kickMember.KickAsync(reason);

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
        /// Kicks a guild member identified by an ID with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.KickMembers"/>` (bot) and `<see cref="Discord.GuildPermission.KickMembers"/>` (user).
        /// <code>
        /// >>kick 336733686529654798 shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("kick", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        [Summary("Kicks a guild member identified by an ID. Optionally with a reason.")]
        public async Task ByID(ulong kickMember, [Remainder] string reason = null) {
            SocketGuildUser kickGuildMember = Context.Guild.GetUser(kickMember);
            if (Context.Guild.GetUser(kickMember) != null) await ByMention(kickGuildMember, reason);
            else {
                DialogContext dialogContext = new DialogContext();
                dialogContext.Guild = Context.Guild;
                dialogContext.Channel = Context.Channel;
                dialogContext.UserAction = DialogContext.Action.Kick;
                dialogContext.Issuer = Context.User;
                dialogContext.Victim = kickGuildMember;
                dialogContext.RequiredGuildPermission = GuildPermission.KickMembers;
                dialogContext.Reason = reason;
                dialogContext.Victim = await Program.Client.Rest.GetUserAsync(kickMember);
                dialogContext.Error = Program.Dialogs.Message.Errors.NotInGuild;

                await dialogContext.SendChannel();
            }
            return;
        }

        /// <summary>
        /// Informs the issuer that a kick could not be issued due to lack of bot/user permissions.
        /// <para>
        /// Prerequisites: Lacking role permissions of either `<see cref="Discord.GuildPermission.KickMembers"/>` (bot) or `<see cref="Discord.GuildPermission.KickMembers"/>` (user).
        /// <code>
        /// >>kick
        /// </code>
        /// </para>
        /// </summary>
        [Command("kick", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task KickNoPerms(SocketGuildUser kickMember, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Kick;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = kickMember;
            dialogContext.Reason = reason;

            if (!Context.Guild.GetUser(Program.Client.CurrentUser.Id).GuildPermissions.KickMembers) {
                dialogContext.RequiredGuildPermission = GuildPermission.KickMembers;
                dialogContext.Error = Program.Dialogs.Message.Errors.NoBotPerms;
            } else if (!Context.Guild.GetUser(Context.User.Id).GuildPermissions.KickMembers) {
                dialogContext.RequiredGuildPermission = GuildPermission.KickMembers;
                dialogContext.Error = Program.Dialogs.Message.Errors.NoUserPerms;
            }
            await dialogContext.SendChannel();
            return;
        }

        /// <summary>
        /// Informs the user that a kick cannot be issued in DM's.
        /// </summary>
        [Command("kick", RunMode = RunMode.Async)]
        [RequireContext(ContextType.DM)]
        public async Task DM(IUser kickMember, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Kick;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = kickMember;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>
        /// Informs the issuer that a kick cannot be issued in Group Chat's.
        /// </summary>
        [Command("kick", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Group)]
        public async Task Group(IUser kickMember, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Kick;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = kickMember;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }
    }
}