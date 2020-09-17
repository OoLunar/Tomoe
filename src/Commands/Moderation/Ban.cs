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
        public async Task ByID(ulong victimId, int pruneDays = 7, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = await Program.Client.Rest.GetUserAsync(victimId);
            dialogContext.UserAction = DialogContext.Action.Ban;
            dialogContext.RequiredGuildPermission = GuildPermission.BanMembers;
            dialogContext.Reason = reason;

            SocketGuildUser banMember = Context.Guild.GetUser(victimId);

            // User does not exist.
            if (dialogContext.Victim == null) return;

            // Check for bot self ban.
            else if (dialogContext.Victim.Id == Program.Client.CurrentUser.Id) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedSelfBot;
                await dialogContext.SendChannel();
                return;
            }
            // Check if bot can ban user.
            else if (banMember != null && banMember.Hierarchy >= Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedHierarchyError;
                await dialogContext.SendChannel();
                return;
            }

            try {
                if ((await Context.Guild.GetBanAsync(victimId)) != null)
                    // The user was already banned
                    dialogContext.Error = Program.Dialogs.Message.Errors.UserAlreadyBanned;
            }
            // Unknown ban error, meaning they weren't banned.
            catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 10026) {
                try {
                    // Let the victim know they were banned.
                    await dialogContext.SendDM();
                    dialogContext.Error = null;
                    // Ban the user
                    await Context.Guild.AddBanAsync(victimId, pruneDays, reason);
                }
                // 50007, DM channel could not be opened or failed to send a message.
                catch (Discord.Net.HttpException error2) when(error2.DiscordCode.HasValue && error2.DiscordCode == 50007) {
                    // If the victim cannot be DM'd, let the issuer know.
                    dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
                }
            }

            dialogContext.SendChannel();
            return;
        }

        [Command("ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ByMention(Mention victim, int pruneDays = 7, [Remainder] string reason = null) => await ByID(victim.Id, pruneDays, reason);

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
        public async Task BanNoPerms(SocketGuildUser victim, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Ban;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = victim;
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
        public async Task DM(IUser victim, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Ban;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = victim;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>Informs the issuer that a ban cannot be issued in Group Chats.</summary>
        [Command("ban", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Group)]
        public async Task Group(IUser victim, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Ban;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = victim;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }
    }
}