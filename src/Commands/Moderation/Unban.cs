using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation {
    public class UnBan : InteractiveBase {

        /// <summary>
        /// Unbans a user identified by an ID.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.BanMembers"/>` (bot) and `<see cref="Discord.GuildPermission.BanMembers"/>` (user).
        /// <code>
        /// >>unban 336733686529654798
        /// </code>
        /// </para>
        /// </summary>
        [Command("unban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task ByID(ulong userId, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.UnBan;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = Context.Client.GetUser(userId);
            dialogContext.Reason = reason;

            try {
                await Context.Guild.RemoveBanAsync(userId);
            } catch {
                dialogContext.Error = Program.Dialogs.Message.Errors.UserNotBanned;
                await dialogContext.SendChannel();
                return;
            }

            try {
                dialogContext.SendDM();
            } catch (System.NullReferenceException) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            }

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>
        /// Unbans a user identified by a mention.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.BanMembers"/>` (bot) and `<see cref="Discord.GuildPermission.BanMembers"/>` (user).
        /// <code>
        /// >>unban &lt;@336733686529654798&gt;
        /// </code>
        /// </para>
        /// </summary>
        [Command("unban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task ByMention(ulong userId, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.UnBan;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = Context.Client.GetUser(userId);
            dialogContext.Reason = reason;

            await Context.Guild.RemoveBanAsync(userId);
            try {
                dialogContext.SendDM();
            } catch {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            }

            await dialogContext.SendChannel();
            return;
        }

        public static void ByProgram(ulong guildId, ulong userId, string reason) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Program.Client.GetGuild(guildId);
            dialogContext.Issuer = Program.Client.CurrentUser;
            dialogContext.Victim = Program.Client.GetUser(userId);
            dialogContext.Reason = reason;
            dialogContext.UserAction = DialogContext.Action.UnBan;

            dialogContext.Guild.RemoveBanAsync(userId);
            dialogContext.SendDM();
            return;
        }
    }
}