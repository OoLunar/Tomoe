using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils.Dialog;

namespace Tomoe.Commands.Moderation {
    public class TempBan : InteractiveBase {

        /// <summary>
        /// TempBans a guild member identified by a mention, optionally with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.TempBanMembers"/>` (bot) and `<see cref="Discord.GuildPermission.TempBanMembers"/>` (user).
        /// <code>
        /// >>temp_ban &lt;@336733686529654798&gt; shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("temp_ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        [Summary("[TempBans a user through a mention or id.](https://github.com/OoLunar/Tomoe/tree/master/docs/moderation/temp_ban.md)")]
        [Remarks("Moderation")]
        public async Task ByID(ulong victimId, System.TimeSpan timespan, int pruneDays = 7, [Remainder] string reason = null) {
            Context dialogContext = new Context();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = await Program.Client.Rest.GetUserAsync(victimId);
            dialogContext.UserAction = Tomoe.Utils.Dialog.Context.Action.TempBan;
            dialogContext.RequiredGuildPermission = GuildPermission.BanMembers;
            dialogContext.Reason = reason;

            Tomoe.Utils.Cache.Tasks.AddTask(Tasks.Reminder.Action.UnBan, Context.Guild.Id, Context.Channel.Id, Context.User.Id, System.DateTime.Now + timespan, System.DateTime.UtcNow, null);

            SocketGuildUser banMember = Context.Guild.GetUser(victimId);

            // User does not exist.
            if (dialogContext.Victim == null) return;

            // Check for bot self temp_ban.
            else if (dialogContext.Victim.Id == Program.Client.CurrentUser.Id) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedSelfBot;
                await dialogContext.SendChannel();
                return;
            }
            // Check if bot can temp_ban user.
            else if (banMember != null && banMember.Hierarchy >= Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedHierarchyError;
                await dialogContext.SendChannel();
                return;
            }

            try {
                if (Context.Guild.GetBanAsync(victimId).Result.User != null)
                    // The user was already temp_banned
                    dialogContext.Error = Program.Dialogs.Message.Errors.UserAlreadyBanned;
                else {
                    // Let the victim know they were temp_banned.
                    await dialogContext.SendDM();
                    // TempBan the user
                    await Context.Guild.AddBanAsync(victimId, pruneDays, reason);
                }
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 50007) {
                // If the victim cannot be DM'd, let the issuer know.
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 10026) { }

            dialogContext.SendChannel();
            return;
        }
    }
}