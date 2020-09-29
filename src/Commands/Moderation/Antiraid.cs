using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Tomoe.Commands.Moderation {
    public class Antiraid : InteractiveBase {

        [Command("antiraid")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(Discord.GuildPermission.BanMembers)]
        [RequireBotPermission(Discord.GuildPermission.BanMembers)]
        public async Task Toggle(bool enabled, int joinRate = 5) {
            Tomoe.Utils.Cache.Antiraid.SetInterval(Context.Guild.Id, joinRate);
            Tomoe.Utils.Cache.Antiraid.SetActivated(Context.Guild.Id, enabled);
            Context.Message.AddReactionAsync(new Discord.Emoji(Program.Dialogs.Emotes.Yes));
            return;
        }
    }
}