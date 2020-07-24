using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation {
    public class Mute : InteractiveBase {
        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteInGuildByMention(SocketGuildUser muteMember) {
            //Get person's nick/username.
            string nickname = Tomoe.Utils.DiscordFunctions.GetCommonName(muteMember);

            //Check for selfmute
            if (muteMember.Id == Context.User.Id) {
                if (muteMember.Hierarchy > Context.Guild.GetUser(Program.client.CurrentUser.Id).Hierarchy) {
                    await ReplyAsync("Unfortunately, you're so high in the hierarchy that I cannot mute you. I deeply aplogize.");
                    return;
                } else {
                    await muteMember.RemoveRolesAsync(muteMember.Roles);
                    await muteMember.AddRoleAsync(Context.Guild.GetRole(Tomoe.Utils.Cache.MutedRole.Get(Context.Guild.Id).RoleID));
                    await ReplyAsync("I have self muted you as requested.");
                    return;
                }
            }

            if (muteMember.Hierarchy > Context.Guild.GetUser(Program.client.CurrentUser.Id).Hierarchy) {
                await ReplyAsync($"I refuse. I shall not mute `{nickname}`. Not only do I lack sufficient permissions, but I wouldn't even dare to think of betraying someone I work for.");
                return;
            }
            await muteMember.RemoveRolesAsync(muteMember.Roles.ExceptEveryoneRole());
            await muteMember.AddRoleAsync(Context.Guild.GetRole(Tomoe.Utils.Cache.MutedRole.Get(Context.Guild.Id).RoleID));
            await ReplyAsync($"I have muted `{nickname}` ({muteMember.Id}) as requested.");
        }

        [Command("mute")][RequireUserPermission(GuildPermission.ManageMessages)][RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteInGuildByID(ulong muteMember) => await MuteInGuildByMention(Context.Guild.GetUser(muteMember));

        [Command("mute")]
        public async Task MuteNoPerms(SocketGuildUser muteMember) {
            //Get person's nick/username.
            string nickname = Tomoe.Utils.DiscordFunctions.GetCommonName(muteMember);

            //Talk proudly.
            if (!Context.Guild.GetUser(Program.client.CurrentUser.Id).GuildPermissions.ManageRoles) await ReplyAsync($"I'm lacking permissions to mute `{nickname}`. Specifically the `ManageRoles` permission.");
            else if (!Context.Guild.GetUser(Context.User.Id).GuildPermissions.ManageMessages) await ReplyAsync($"You wish to mute `{nickname}`? Ha! In your dreams. You still have roles and permissions to achieve before you can start doing big actions like that. Get on my level first, then we'll see.");
        }

        [Command("mute")]
        [RequireContext(ContextType.DM)]
        public async Task MuteInDM(IUser muteMember) {
            await ReplyAsync("Muting can only be done in Guilds. Have some common sense.");
            return;
        }

        [Command("mute")]
        [RequireContext(ContextType.Group)]
        public async Task MuteInGroup(IUser muteMember) {
            await ReplyAsync("Muting can only be done in Guilds. Have some common sense.");
            return;
        }
    }
}