using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Tomoe.Commands.Setup {
    public class Logging : InteractiveBase {
        [Command("setup_message_logging", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ByID(ulong channelID) {
            Utils.Cache.Guild.AddLoggingChannel(Context.Guild.Id, Event.MessageUpdated, channelID);
            Context.Message.AddReactionAsync(new Emoji("👍"));
        }

        [Command("setup_message_logging", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ByMention(ITextChannel channel) {
            Utils.Cache.Guild.AddLoggingChannel(Context.Guild.Id, Event.MessageUpdated, channel.Id);
            Context.Message.AddReactionAsync(new Emoji("👍"));
        }
    }
}