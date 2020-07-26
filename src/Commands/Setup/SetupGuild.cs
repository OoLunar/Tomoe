using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Tomoe.Utils;
using Tomoe.Utils.Cache;

namespace Tomoe.Commands.Moderation {
    public class SetupGuild : InteractiveBase {
        private Dictionary<string, string[]> setupGuildDialog = Program.Dialogs.GuildSetup;

        [Command("setup_guild", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetupGuildAdmin() {
            if (SetupGuildCache.Get(Context.Guild.Id) != null) await ReplyAsync(setupGuildDialog.GetRandomValue("already_setup"));
            else {
                SetupGuildCache.Store(Context.Guild.Id);
                await ReplyAsync(setupGuildDialog.GetRandomValue("setup_complete"));
            }
        }
    }
}