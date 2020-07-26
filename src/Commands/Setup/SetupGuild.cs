using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Tomoe.Utils.Cache;

namespace Tomoe.Commands.Moderation {
    public class SetupGuild : InteractiveBase {
        private Dictionary<string, string[]> SetupGuildDialog = Program.Dialogs.GuildSetup;

        [Command("setup_guild", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetupGuildAdmin() {
            if (SetupGuildCache.Get(Context.Guild.Id) != null) await ReplyAsync();
            else {
                SetupGuildCache.Store(Context.Guild.Id);
                await ReplyAsync("Done.");
            }
        }
    }
}