using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;
using Tomoe.Utils.Cache;

namespace Tomoe.Commands.Setup {
    public class Mute : InteractiveBase {

        /// <summary>
        /// Setup the mute role by pinging said role.
        /// <para>
        /// <code>
        /// >>setup_mute &lt;@&amp;role_id&gt;
        /// </code>
        /// </para>
        /// </summary>
        [Command("setup_mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetupMuteByRole(IRole role) {
            MutedRole roleSet = MutedRole.Get(Context.Guild.Id);
            if (roleSet == null) {
                MutedRole.Store(Context.Guild.Id, role.Id, Context.User.Id);
                foreach (IGuildChannel channel in Context.Guild.Channels) {
                    if (channel is ITextChannel)
                        await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny, embedLinks: PermValue.Deny, attachFiles: PermValue.Deny, mentionEveryone: PermValue.Deny, useExternalEmojis: PermValue.Deny));
                }
                await ReplyAsync("Done.");
            } else if (roleSet.RoleID == role.Id) {
                await ReplyAsync("Already set as mute role");
            } else {
                await ReplyAsync("There's already a mute role setup. Continue anyways? Y | N");
                SocketMessage userResponse = await NextMessageAsync();
                if (userResponse.ToString().ToLower() == "y") {
                    foreach (IGuildChannel channel in Context.Guild.Channels) {
                        if (channel is ITextChannel) {
                            await channel.RemovePermissionOverwriteAsync(Context.Guild.GetRole(roleSet.RoleID));
                            await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny, embedLinks: PermValue.Deny, attachFiles: PermValue.Deny, mentionEveryone: PermValue.Deny, useExternalEmojis: PermValue.Deny));
                        }
                    }
                    MutedRole.Store(Context.Guild.Id, role.Id, Context.User.Id);
                    await ReplyAsync("Done.");
                } else await ReplyAsync("Exited.");
            }
        }

        /// <summary>
        /// Setup the mute role by using said role's ID.
        /// <para>
        /// <code>
        /// >>setup_mute role_id
        /// </code>
        /// </para>
        /// </summary>
        [Command("setup_mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetupMuteByID(ulong role) => await SetupMuteByRole(Context.Guild.GetRole(role));

        /// <summary>
        /// Get's the current mute role.
        /// <para>
        /// <code>
        /// >>setup_mute
        /// </code>
        /// </para>
        /// </summary>
        [Command("setup_mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CheckMuteRole() {
            MutedRole roleSet = MutedRole.Get(Context.Guild.Id);
            if (roleSet == null) await ReplyAsync(Program.Dialogs.MuteSetup.GetRandomValue("not_setup"));
            else {
                var user = Context.Guild.GetUser(roleSet.UserID);
                string isSetupDialog = Program.Dialogs.MuteSetup.GetRandomValue("is_setup").Replace("$administrator", Context.Guild.GetUser(roleSet.UserID).Mention).Replace("$role", Context.Guild.GetRole(roleSet.RoleID).Mention);
                await ReplyAsync(isSetupDialog);
            }
        }
    }
}