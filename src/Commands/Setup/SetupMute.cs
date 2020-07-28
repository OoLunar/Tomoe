using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils.Cache;

namespace Tomoe.Commands.Setup {
    public class Mute : InteractiveBase {
        [Command("setup_mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetupMuteRole(IRole role) {
            MutedRole roleSet = MutedRole.Get(Context.Guild.Id);
            if (roleSet == null) {
                MutedRole.Store(Context.Guild.Id, role.Id, Context.User.Id);
                foreach (IGuildChannel channel in Context.Guild.Channels) {
                    if (channel is ITextChannel) await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny, embedLinks: PermValue.Deny, attachFiles: PermValue.Deny, mentionEveryone: PermValue.Deny, useExternalEmojis: PermValue.Deny));
                }
                await ReplyAsync("Done.");
            } else if (roleSet.RoleID == role.Id) {
                await ReplyAsync("Already set as mute role");
            } else {
                await ReplyAsync("There's already a mute role setup. Continue anyways? Y | N");
                SocketMessage userResponse = await NextMessageAsync();
                if (userResponse.ToString().ToLower() == "y") {
                    //Add new one
                    foreach (IGuildChannel channel in Context.Guild.Channels) {
                        if (channel is ITextChannel) {
                            //Remove old role
                            await channel.RemovePermissionOverwriteAsync(Context.Guild.GetRole(roleSet.RoleID));
                            //Add new one
                            await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny, embedLinks: PermValue.Deny, attachFiles: PermValue.Deny, mentionEveryone: PermValue.Deny, useExternalEmojis: PermValue.Deny));
                        }
                    }
                    MutedRole.Store(Context.Guild.Id, role.Id, Context.User.Id);
                    await ReplyAsync("Done.");
                } else await ReplyAsync("Exited.");
            }
        }
    }
}