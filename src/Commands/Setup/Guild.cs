using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Tomoe.Utils;
using Tomoe.Utils.Cache;

namespace Tomoe.Commands.Setup {
    public class Guild : InteractiveBase {
        /// <summary>All the dialogs for the <see cref="Tomoe.Commands.Setup.Guild"/> class.</summary>
        private Dictionary<string, string[]> setupGuildDialog = Program.Dialogs.GuildSetup;

        /// <summary>
        /// Sets up the Discord Guild by insert the guild id into the database through <see cref="Tomoe.Utils.Cache.SetupGuild"/>
        /// <para>
        /// <code>
        /// >>setup_guild
        /// </code>
        /// </para>
        /// </summary>
        [Command("setup_guild", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Admin() {
            if (SetupGuild.Get(Context.Guild.Id) != null) {
                await ReplyAsync(setupGuildDialog.GetRandomValue("is_setup").DialogSetParams(Context));
            } else {
                SetupGuild.Store(Context.Guild.Id);
                await ReplyAsync(setupGuildDialog.GetRandomValue("success_setup").DialogSetParams(Context));
            }
        }

        [Command("setup guild", RunMode = RunMode.Async)]
        public async Task NoPerms() {
            await ReplyAsync(setupGuildDialog.GetRandomValue("failed_perms"));
        }
    }
}