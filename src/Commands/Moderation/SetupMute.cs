using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils.Cache;

namespace Tomoe.Commands.Moderation {
    public class SetupMute : InteractiveBase {
        [Command("setup_mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetupMuteRole(IRole role) {
            MutedRole roleSet = MutedRole.Get(Context.Guild.Id);
            if (roleSet == null) {
                MutedRole.Store(Context.Guild.Id, role.Id, Context.User.Id);
                await ReplyAsync("Done.");
            } else if (roleSet.RoleID == role.Id) {
                await ReplyAsync("Already set as mute role");
            } else {
                await ReplyAsync("There's already a mute role setup. Continue anyways? Y | N");
                SocketMessage userResponse = await NextMessageAsync();
                if (userResponse.ToString().ToLower() == "y") {
                    MutedRole.Store(Context.Guild.Id, role.Id, Context.User.Id);
                    await ReplyAsync("Done.");
                } else await ReplyAsync("Exited.");
            }
        }
    }
}