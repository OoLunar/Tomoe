using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation {
    public class Mute : InteractiveBase {

        private Dictionary<string, string[]> muteDialogs = Program.Dialogs.Mute;

        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByMention(SocketGuildUser muteMember, string reason) {
            string nickname = Tomoe.Utils.DiscordFunctions.GetCommonName(muteMember);

            if (muteMember.Id == Context.User.Id) {
                if (muteMember.Hierarchy > Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                    await ReplyAsync(muteDialogs.GetRandomValue("failed_self_mute").DialogSetParams(Context.Guild.GetUser(Context.User.Id), muteMember, reason));
                    return;
                } else {
                    await muteMember.RemoveRolesAsync(muteMember.Roles);
                    await muteMember.AddRoleAsync(Context.Guild.GetRole(Tomoe.Utils.Cache.MutedRole.Get(Context.Guild.Id).RoleID));
                    await ReplyAsync(muteDialogs.GetRandomValue("success_issuer").DialogSetParams(Context.Guild.GetUser(Context.User.Id), muteMember, reason));
                    return;
                }
            }

            if (muteMember.Hierarchy > Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                await ReplyAsync(muteDialogs.GetRandomValue("hierarchy_error").DialogSetParams(Context.Guild.GetUser(Context.User.Id), muteMember, reason));
                return;
            }
            await muteMember.RemoveRolesAsync(muteMember.Roles.ExceptEveryoneRole());
            await muteMember.AddRoleAsync(Context.Guild.GetRole(Tomoe.Utils.Cache.MutedRole.Get(Context.Guild.Id).RoleID));
            await ReplyAsync(muteDialogs.GetRandomValue("success_victim").DialogSetParams(Context.Guild.GetUser(Context.User.Id), muteMember, reason));
        }

        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByID(ulong muteMember, string reason) => await ByMention(Context.Guild.GetUser(muteMember), reason);

        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByIDNoReason(ulong muteMember) => await ByMention(Context.Guild.GetUser(muteMember), null);

        /// <summary>
        ///  Mutes a guild member without a reason. An alias to <see cref="Tomoe.Commands.Moderation.Mute.ByMention(SocketGuildUser, string)">.
        /// </summary>
        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByMentionNoReason(SocketGuildUser muteMember) => await ByMention(muteMember, null);

        [Command("mute")]
        public async Task MuteNoPerms(SocketGuildUser muteMember) {
            //Get person's nick/username.
            string nickname = Tomoe.Utils.DiscordFunctions.GetCommonName(muteMember);

            //Talk proudly.
            if (!Context.Guild.GetUser(Program.Client.CurrentUser.Id).GuildPermissions.ManageRoles) await ReplyAsync(muteDialogs.GetRandomValue("lack_of_bot_perms").DialogSetParams(Context.Guild.GetUser(Context.User.Id), muteMember, null));
            else if (!Context.Guild.GetUser(Context.User.Id).GuildPermissions.ManageMessages) await ReplyAsync(muteDialogs.GetRandomValue("lack_of_user_perms").DialogSetParams(Context.Guild.GetUser(Context.User.Id), muteMember, null));
        }

        [Command("mute")]
        [RequireContext(ContextType.DM)]
        public async Task DM(IUser muteMember) {
            await ReplyAsync("Muting can only be done in Guilds. Have some common sense.");
            return;
        }

        [Command("mute")]
        [RequireContext(ContextType.Group)]
        public async Task Group(IUser muteMember) {
            await ReplyAsync("Muting can only be done in Guilds. Have some common sense.");
            return;
        }
    }
}