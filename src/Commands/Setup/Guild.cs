using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Tomoe.Utils;

namespace Tomoe.Commands.Setup {
    public class Guild : InteractiveBase {
        /// <summary>All the dialogs for the <see cref="Tomoe.Commands.Setup.Guild"/> class.</summary>
        private Utils.Classes.Guild setupGuildDialog = Program.Dialogs.Message.Setup.Guild;

        /// <summary>
        /// Sets up the Discord Guild by insert the guild id into the database through <see cref="Tomoe.Utils.Cache.Guild"/>
        /// <para>
        /// <code>
        /// >>setup_guild
        /// </code>
        /// </para>
        /// </summary>
        [Command("setup_guild", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Admin() {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.Issuer = Context.Guild.GetUser(Context.User.Id);

            if (Utils.Cache.Guild.Get(Context.Guild.Id) != null) {
                dialogContext.Error = setupGuildDialog.AlreadySetup;
            } else {
                Utils.Cache.Guild.Store(Context.Guild.Id);
            }
            dialogContext.SendChannel();
        }

        [Command("setup_guild", RunMode = RunMode.Async)]
        public async Task NoPerms() {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.Issuer = Context.Guild.GetUser(Context.User.Id);
            dialogContext.Error = setupGuildDialog.FailedUserPermissions;

            dialogContext.SendChannel();
        }
    }
}