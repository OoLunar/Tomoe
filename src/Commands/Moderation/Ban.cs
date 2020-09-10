using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation {
    public class Ban : InteractiveBase {

        /// <summary>
        /// Bans a guild member identified by a mention, optionally with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.BanMembers"/>` (bot) and `<see cref="Discord.GuildPermission.BanMembers"/>` (user).
        /// <code>
        /// >>ban &lt;@336733686529654798&gt; shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [Summary("[Bans a user through a mention or id.](https://github.com/OoLunar/Tomoe/tree/master/docs/moderation/ban.md)")]
        [Remarks("Moderation")]
        public async Task ByMention(SocketGuildUser banMember, int pruneDays = 7, [Remainder] string reason = null) {
            // Add dialog context.
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Ban;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = banMember;
            dialogContext.Reason = reason;

            // Check if the user is already banned.

            try {
                // Will throw an error if user is not banned.
                await Context.Guild.GetBanAsync(banMember);
                dialogContext.Error = Program.Dialogs.Message.Errors.UserAlreadyBanned;
                await dialogContext.SendChannel();
                return;
            } catch (Discord.Net.HttpException e) when(e.DiscordCode.HasValue && e.DiscordCode == 10026) { }

            // Check for bot self ban.
            if (banMember.Id == Program.Client.CurrentUser.Id) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedSelfBot;
                await dialogContext.SendChannel();
                return;
            }
            // Check if bot can ban user.
            else if (banMember.Hierarchy >= Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedHierarchyError;
                await dialogContext.SendChannel();
                return;
            }

            await Context.Guild.AddBanAsync(banMember.Id, pruneDays, reason);

            try {
                dialogContext.SendDM();
            } catch {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            }

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>
        /// Bans a guild member identified by an ID with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.BanMembers"/>` (bot) and `<see cref="Discord.GuildPermission.BanMembers"/>` (user).
        /// <code>
        /// >>ban 336733686529654798 shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ByID(ulong banMember, int pruneDays = 7, [Remainder] string reason = null) {
            SocketGuildUser banGuildMember = Context.Guild.GetUser(banMember);
            if (Context.Guild.GetUser(banMember) != null) await ByMention(banGuildMember, pruneDays, reason);
            else {
                DialogContext dialogContext = new DialogContext();
                dialogContext.Guild = Context.Guild;
                dialogContext.Channel = Context.Channel;
                dialogContext.UserAction = DialogContext.Action.Ban;
                dialogContext.Issuer = Context.User;
                dialogContext.Victim = banGuildMember;
                dialogContext.RequiredGuildPermission = GuildPermission.BanMembers;
                dialogContext.Reason = reason;
                dialogContext.Victim = await Program.Client.Rest.GetUserAsync(banMember);
                dialogContext.Error = Program.Dialogs.Message.Errors.NotInGuild;

                await dialogContext.SendChannel();
            }
            return;
        }

        /// <summary>
        /// Informs the issuer that a ban could not be issued due to lack of bot/user permissions.
        /// <para>
        /// Prerequisites: Lacking role permissions of either `<see cref="Discord.GuildPermission.BanMembers"/>` (bot) or `<see cref="Discord.GuildPermission.BanMembers"/>` (user).
        /// <code>
        /// >>ban
        /// </code>
        /// </para>
        /// </summary>
        [Command("ban", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task BanNoPerms(SocketGuildUser banMember, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Ban;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = banMember;
            dialogContext.Reason = reason;
            dialogContext.RequiredGuildPermission = GuildPermission.BanMembers;

            if (!Context.Guild.GetUser(Program.Client.CurrentUser.Id).GuildPermissions.BanMembers)
                dialogContext.Error = Program.Dialogs.Message.Errors.NoBotPerms;
            else if (!Context.Guild.GetUser(Context.User.Id).GuildPermissions.BanMembers)
                dialogContext.Error = Program.Dialogs.Message.Errors.NoUserPerms;

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>Informs the user that a ban cannot be issued in DMs.</summary>
        [Command("ban", RunMode = RunMode.Async)]
        [RequireContext(ContextType.DM)]
        public async Task DM(IUser banMember, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Ban;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = banMember;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>Informs the issuer that a ban cannot be issued in Group Chats.</summary>
        [Command("ban", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Group)]
        public async Task Group(IUser banMember, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Ban;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = banMember;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }
    }
}