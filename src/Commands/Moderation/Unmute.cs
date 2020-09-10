/*
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation {
    public class Unmute : InteractiveBase {
        //TODO: Make this actually unmute instead of unban.

        /// <summary>
        /// Unbans a user identified by an ID.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.BanMembers"/>` (bot) and `<see cref="Discord.GuildPermission.BanMembers"/>` (user).
        /// <code>
        /// >>unban 336733686529654798
        /// </code>
        /// </para>
        /// </summary>
        [Command("unmute")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [Summary("[Unmutes a user by mention or ID. Restores their previous roles.](https://github.com/OoLunar/Tomoe/tree/master/docs/moderation/unmute.md)")]
        [Remarks("Moderation")]
        public async Task ByID(ulong userId) {
            await Context.Guild.RemoveBanAsync(userId);
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
        [Command("unmute")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task ByMention(ulong userId, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = Context.Client.GetUser(userId);
            dialogContext.Reason = reason;
            dialogContext.UserAction = DialogContext.Action.UnBan;

            await Context.Guild.RemoveBanAsync(userId);
            try {
                dialogContext.SendDM();
            } catch {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
                await dialogContext.SendChannel();
                return;
            }

            dialogContext.SendChannel();
        }

        public static async Task ByProgram(ulong guildId, ulong userId) => await Program.Client.GetGuild(guildId).RemoveBanAsync(userId);
    }
}
*/