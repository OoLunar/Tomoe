using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils.Cache;

namespace Tomoe.Commands.Moderation {
    public class SetupGuild : InteractiveBase {
        [Command("setup_guild", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetupGuildAdmin() {
            if (SetupGuildCache.Get(Context.Guild.Id) != null) await ReplyAsync($"{Context.Guild.Name} is already setup.");
            else {
                SetupGuildCache.Store(Context.Guild.Id);
                await ReplyAsync("Done.");
            }
        }
    }
}