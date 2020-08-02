using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation {
    public class Kick : InteractiveBase {
        private Dictionary<string, string[]> kickDialogs = Program.Dialogs.Kick;

        /// <summary>
        /// Kciks a guild member identified by a mention with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.KickMembers"/>` (bot) and `<see cref="Discord.GuildPermission.KickMembers"/>` (user).
        /// <code>
        /// >>kick &lt;@336733686529654798&gt; shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("kick")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByMention(SocketGuildUser kickMember, string reason) {
            SocketGuildUser issuer = Context.Guild.GetUser(Context.User.Id);
            string nickname = Tomoe.Utils.DiscordFunctions.GetCommonName(kickMember);
            ulong muteRole = Tomoe.Utils.Cache.MutedRole.Get(Context.Guild.Id).RoleID;

            if (kickMember.Id == Context.User.Id) {
                if (kickMember.Hierarchy > Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                    await ReplyAsync(kickDialogs.GetRandomValue("failed_self_kick").DialogSetParams(Context, kickMember, reason));
                    return;
                } else {
                    await kickMember.RemoveRolesAsync(kickMember.Roles);
                    await kickMember.AddRoleAsync(Context.Guild.GetRole(muteRole));
                    await ReplyAsync(kickDialogs.GetRandomValue("success_issuer").DialogSetParams(Context, kickMember, reason));
                    return;
                }
            }

            if (kickMember.Hierarchy > Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                await ReplyAsync(kickDialogs.GetRandomValue("hierarchy_error").DialogSetParams(Context, kickMember, reason));
                return;
            }
            await kickMember.RemoveRolesAsync(kickMember.Roles.ExceptEveryoneRole());
            await kickMember.AddRoleAsync(Context.Guild.GetRole(muteRole));
            await ReplyAsync(kickDialogs.GetRandomValue("success_victim").DialogSetParams(Context, kickMember, reason));
        }

        /// <summary>
        /// Kicks a guild member identified by a mention, without a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.KickMembers"/>` (bot) and `<see cref="Discord.GuildPermission.KickMembers"/>` (user).
        /// <code>
        /// >>kick &lt;@336733686529654798&gt;
        /// </code>
        /// </para>
        /// </summary>
        [Command("kick")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByMentionNoReason(SocketGuildUser kickMember) => await ByMention(kickMember, null);

        /// <summary>
        /// Kicks a guild member identified by an ID with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.KickMembers"/>` (bot) and `<see cref="Discord.GuildPermission.KickMembers"/>` (user).
        /// <code>
        /// >>kick 336733686529654798 shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("kick")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByID(ulong kickMember, string reason) => await ByMention(Context.Guild.GetUser(kickMember), reason);

        /// <summary>
        /// Kicks a guild member identified by a mention without a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.KickMembers"/>` (bot) and `<see cref="Discord.GuildPermission.KickMembers"/>` (user).
        /// <code>
        /// >>kick 336733686529654798
        /// </code>
        /// </para>
        /// </summary>
        [Command("kick")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByIDNoReason(ulong kickMember) => await ByMention(Context.Guild.GetUser(kickMember), null);

        /// <summary>
        /// Informs the issuer that a kick could not be issued due to lack of bot/user permissions.
        /// <para>
        /// Prerequisites: Lacking role permissions of either `<see cref="Discord.GuildPermission.KickMembers"/>` (bot) or `<see cref="Discord.GuildPermission.KickMembers"/>` (user).
        /// <code>
        /// >>kick
        /// </code>
        /// </para>
        /// </summary>
        [Command("kick")]
        public async Task KickNoPerms(SocketGuildUser kickMember) {
            string nickname = Tomoe.Utils.DiscordFunctions.GetCommonName(kickMember);
            if (!Context.Guild.GetUser(Program.Client.CurrentUser.Id).GuildPermissions.ManageRoles)
                await ReplyAsync(kickDialogs.GetRandomValue("lack_of_bot_perms").DialogSetParams(Context.Guild.GetUser(Context.User.Id), kickMember, null));

            else if (!Context.Guild.GetUser(Context.User.Id).GuildPermissions.ManageMessages)
                await ReplyAsync(kickDialogs.GetRandomValue("lack_of_user_perms").DialogSetParams(Context.Guild.GetUser(Context.User.Id), kickMember, null));
        }

        /// <summary>
        /// Informs the user that a kick cannot be issued in DM's.
        /// </summary>
        [Command("kick")]
        [RequireContext(ContextType.DM)]
        public async Task DM(IUser kickMember) {
            await ReplyAsync(kickDialogs.GetRandomValue("dm").DialogSetParams(Context.Guild.GetUser(Context.User.Id), kickMember, null));
            return;
        }

        /// <summary>
        /// Informs the issuer that a kick cannot be issued in Group Chat's.
        /// </summary>
        [Command("hard_mute")]
        [RequireContext(ContextType.Group)]
        public async Task Group(IUser kickMember) {
            await ReplyAsync(kickDialogs.GetRandomValue("dm").DialogSetParams(Context.Guild.GetUser(Context.User.Id), kickMember, null));
            return;
        }
    }
}