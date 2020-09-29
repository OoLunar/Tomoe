using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Tomoe.Utils.Dialog;

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
        [Summary("[Unbans a user by a mention or ID](https://github.com/OoLunar/Tomoe/tree/master/docs/moderation/unban.md)")]
        [Remarks("Moderation")]
        public async Task ByID(ulong victimID, [Remainder] string reason = null) {
            Context dialogContext = new Context();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = await Program.Client.Rest.GetUserAsync(victimID);
            dialogContext.UserAction = Tomoe.Utils.Dialog.Context.Action.UnBan;
            dialogContext.RequiredGuildPermission = GuildPermission.BanMembers;
            dialogContext.Reason = reason;

            // User does not exist.
            if (dialogContext.Victim == null) return;

            try {
                // Remove the ban.
                if ((await Context.Guild.GetBanAsync(victimID)) != null) await Context.Guild.RemoveBanAsync(victimID);
                // Let the victim know the ban was removed.
                await dialogContext.SendDM();
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 50007) {
                // If the victim cannot be DM'd, let the issuer know.
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 10026) {
                // If the user was never banned in the first place.
                dialogContext.Error = Program.Dialogs.Message.Errors.UserNotBanned;
            }

            dialogContext.SendChannel();
            return;
        }

        [Command("unban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task ByMention(Mention user, [Remainder] string reason = null) => await ByID(user.Id, reason);

        public static async Task ByProgram(ulong guildId, ulong userID, string reason) {
            Context dialogContext = new Context();
            dialogContext.Guild = await Program.Client.Rest.GetGuildAsync(guildId);
            dialogContext.Issuer = Program.Client.CurrentUser;
            dialogContext.Victim = await Program.Client.Rest.GetUserAsync(userID);
            dialogContext.UserAction = Tomoe.Utils.Dialog.Context.Action.UnBan;
            dialogContext.RequiredGuildPermission = GuildPermission.BanMembers;
            dialogContext.Reason = reason;

            // User does not exist.
            if (dialogContext.Victim == null) return;

            try {
                // Remove the ban.
                if ((await dialogContext.Guild.GetBanAsync(userID)) != null) await dialogContext.Guild.RemoveBanAsync(userID);
                // Let the victim know the ban was removed.
                await dialogContext.SendDM();
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 50007) {
                // If the victim cannot be DM'd, let the issuer know.
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 10026) {
                // If the user was never banned in the first place.
                dialogContext.Error = Program.Dialogs.Message.Errors.UserNotBanned;
            }

            return;
        }
    }
}