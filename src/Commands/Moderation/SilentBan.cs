using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils.Dialog;

namespace Tomoe.Commands.Moderation {
    public class SilentBan : InteractiveBase {

        /// <summary>
        /// Silently bans someone by an id from the guild without any trace in chat. Still gets logged in the modlog channel.
        /// <para>
        /// Prerequisites: Lacking role permissions of either `<see cref="Discord.GuildPermission.BanMembers"/>` (bot) or `<see cref="Discord.GuildPermission.BanMembers"/>` (user).
        /// <code>
        /// >>silent_ban 336733686529654798 shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("silent_ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [Summary("[Silently bans a user by a mention or ID](https://github.com/OoLunar/Tomoe/tree/master/docs/moderation/silent_ban.md)")]
        [Remarks("Moderation")]
        public async Task ByID(ulong victimId, int pruneDays = 7, string reason = null) {

            Context dialogContext = new Context();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = await Program.Client.Rest.GetUserAsync(victimId);
            dialogContext.UserAction = Tomoe.Utils.Dialog.Context.Action.Ban;
            dialogContext.RequiredGuildPermission = GuildPermission.BanMembers;
            dialogContext.Reason = reason;

            SocketGuildUser banMember = Context.Guild.GetUser(victimId);

            // User does not exist.
            if (dialogContext.Victim == null) return;
            // Check for bot self ban.
            else if (dialogContext.Victim.Id == Program.Client.CurrentUser.Id) return;
            // Check if bot can ban user.
            else if (banMember != null && banMember.Hierarchy >= Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) return;

            try {
                // The user was already banned
                if (Context.Guild.GetBanAsync(victimId).Result.User != null) return;
                else await Context.Guild.AddBanAsync(victimId, pruneDays, reason);
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 10026) { }

            Context.Message.AddReactionAsync(new Emoji("👍"));
            System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(5));
            await Context.Message.DeleteAsync();
            return;
        }

        /// <summary>
        /// Silently bans someone by mention from the guild without any trace in chat. Still gets logged in the modlog channel.
        /// <para>
        /// Prerequisites: Lacking role permissions of either `<see cref="Discord.GuildPermission.BanMembers"/>` (bot) or `<see cref="Discord.GuildPermission.BanMembers"/>` (user).
        /// <code>
        /// >>silent_ban &lt;@336733686529654798&gt; shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("silent_ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ByMention(IGuildUser user, int pruneDays = 7, string reason = null) => await ByID(user.Id, pruneDays, reason);

        /// <summary>
        /// Informs the issuer that a silent ban could not be issued due to lack of bot/user permissions.
        /// <para>
        /// Prerequisites: Lacking role permissions of either `<see cref="Discord.GuildPermission.BanMembers"/>` (bot) or `<see cref="Discord.GuildPermission.BanMembers"/>` (user).
        /// <code>
        /// >>silent_ban
        /// </code>
        /// </para>
        /// </summary>
        [Command("silent_ban", RunMode = RunMode.Async)]
        public async Task BanNoPerms(SocketGuildUser banMember, [Remainder] string reason = null) {
            Context dialogContext = new Context();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = Tomoe.Utils.Dialog.Context.Action.Ban;
            dialogContext.RequiredGuildPermission = GuildPermission.BanMembers;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = banMember;
            dialogContext.Reason = reason;

            if (!Context.Guild.GetUser(Program.Client.CurrentUser.Id).GuildPermissions.BanMembers)
                dialogContext.Error = Program.Dialogs.Message.Errors.NoBotPerms;
            else if (!Context.Guild.GetUser(Context.User.Id).GuildPermissions.BanMembers)
                dialogContext.Error = Program.Dialogs.Message.Errors.NoUserPerms;

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>Informs the user that a ban cannot be issued in DMs.</summary>
        [Command("silent_ban", RunMode = RunMode.Async)]
        [RequireContext(ContextType.DM)]
        public async Task DM(IUser banMember, [Remainder] string reason = null) {
            Context dialogContext = new Context();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = Tomoe.Utils.Dialog.Context.Action.Ban;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = banMember;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>Informs the issuer that a ban cannot be issued in Group Chats.</summary>
        [Command("silent_ban", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Group)]
        public async Task Group(IUser banMember, [Remainder] string reason = null) {
            Context dialogContext = new Context();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = Tomoe.Utils.Dialog.Context.Action.Ban;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = banMember;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }
    }
}