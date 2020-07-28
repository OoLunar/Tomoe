using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation {
    public class HardMute : InteractiveBase {
        private Dictionary<string, string[]> muteDialogs = Program.Dialogs.Mute;

        /// <summary>
        /// Mutes a guild member identified by a mention with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.ManageRoles"/>` (bot) and `<see cref="Discord.GuildPermission.ManageMessages"/>` (user).
        /// <code>
        /// >>mute &lt;@336733686529654798&gt; shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("hard_mute")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByMention(SocketGuildUser muteMember, string reason) {
            SocketGuildUser issuer = Context.Guild.GetUser(Context.User.Id);
            string nickname = Tomoe.Utils.DiscordFunctions.GetCommonName(muteMember);
            ulong muteRole = Tomoe.Utils.Cache.MutedRole.Get(Context.Guild.Id).RoleID;

            if (muteMember.Id == Context.User.Id) {
                if (muteMember.Hierarchy > Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                    await ReplyAsync(muteDialogs.GetRandomValue("failed_self_mute").DialogSetParams(Context, muteMember, reason));
                    return;
                } else {
                    await muteMember.RemoveRolesAsync(muteMember.Roles);
                    await muteMember.AddRoleAsync(Context.Guild.GetRole(muteRole));
                    await ReplyAsync(muteDialogs.GetRandomValue("success_issuer").DialogSetParams(Context, muteMember, reason));
                    return;
                }
            }

            if (muteMember.Hierarchy > Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                await ReplyAsync(muteDialogs.GetRandomValue("hierarchy_error").DialogSetParams(Context, muteMember, reason));
                return;
            }
            await muteMember.RemoveRolesAsync(muteMember.Roles.ExceptEveryoneRole());
            await muteMember.AddRoleAsync(Context.Guild.GetRole(muteRole));
            await ReplyAsync(muteDialogs.GetRandomValue("success_victim").DialogSetParams(Context, muteMember, reason));
        }

        /// <summary>
        /// Mutes a guild member identified by a mention.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.ManageRoles"/>` (bot) and `<see cref="Discord.GuildPermission.ManageMessages"/>` (user).
        /// <code>
        /// >>mute &lt;@336733686529654798&gt;
        /// </code>
        /// </para>
        /// </summary>
        [Command("hard_mute")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByMentionNoReason(SocketGuildUser muteMember) => await ByMention(muteMember, null);

        /// <summary>
        /// Mutes a guild member identified by an ID with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.ManageRoles"/>` (bot) and `<see cref="Discord.GuildPermission.ManageMessages"/>` (user).
        /// <code>
        /// >>mute 336733686529654798 shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("hard_mute")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByID(ulong muteMember, string reason) => await ByMention(Context.Guild.GetUser(muteMember), reason);

        /// <summary>
        /// Mutes a guild member identified by a mention without a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.ManageRoles"/>` (bot) and `<see cref="Discord.GuildPermission.ManageMessages"/>` (user).
        /// <code>
        /// >>mute 336733686529654798
        /// </code>
        /// </para>
        /// </summary>
        [Command("hard_mute")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByIDNoReason(ulong muteMember) => await ByMention(Context.Guild.GetUser(muteMember), null);

        /// <summary>
        /// Informs the issuer that a mute could not be issued due to lack of bot/user permissions.
        /// <para>
        /// Prerequisites: Lacking role permissions of either `<see cref="Discord.GuildPermission.ManageRoles"/>` (bot) or `<see cref="Discord.GuildPermission.ManageMessages"/>` (user).
        /// <code>
        /// >>mute
        /// </code>
        /// </para>
        /// </summary>
        [Command("hard_mute")]
        public async Task MuteNoPerms(SocketGuildUser muteMember) {
            string nickname = Tomoe.Utils.DiscordFunctions.GetCommonName(muteMember);
            if (!Context.Guild.GetUser(Program.Client.CurrentUser.Id).GuildPermissions.ManageRoles)
                await ReplyAsync(muteDialogs.GetRandomValue("lack_of_bot_perms").DialogSetParams(Context.Guild.GetUser(Context.User.Id), muteMember, null));

            else if (!Context.Guild.GetUser(Context.User.Id).GuildPermissions.ManageMessages)
                await ReplyAsync(muteDialogs.GetRandomValue("lack_of_user_perms").DialogSetParams(Context.Guild.GetUser(Context.User.Id), muteMember, null));
        }

        /// <summary>
        /// Informs the user that mutes cannot be issued in DM's.
        /// </summary>
        [Command("hard_mute")]
        [RequireContext(ContextType.DM)]
        public async Task DM(IUser muteMember) {
            await ReplyAsync(muteDialogs.GetRandomValue("dm").DialogSetParams(Context.Guild.GetUser(Context.User.Id), muteMember, null));
            return;
        }

        /// <summary>
        /// Informs the issuer that a mute cannot be issued in Group Chat's.
        /// </summary>
        [Command("hard_mute")]
        [RequireContext(ContextType.Group)]
        public async Task Group(IUser muteMember) {
            await ReplyAsync(muteDialogs.GetRandomValue("dm").DialogSetParams(Context.Guild.GetUser(Context.User.Id), muteMember, null));
            return;
        }
    }
}