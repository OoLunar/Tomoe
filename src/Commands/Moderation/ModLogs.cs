using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace Tomoe.Commands.Moderation
{
    public class ModLogs : BaseCommandModule
    {
        [Command("modlog"), Description("Logs something to the modlog."), Aliases("mod_log", "log", "ml", "mod_logs", "modlogs", "mls"), RequireUserPermissions(Permissions.ManageGuild)]
        public async Task Overload(CommandContext context, [Description("More details on what's being recorded."), RemainingText] string reason = Constants.MissingReason)
        {
            await Api.Moderation.ModLog(context.Guild, Api.Moderation.LogType.CustomEvent, null, reason);
            await Program.SendMessage(context, "Successfully recorded event into the ModLog.");
        }
    }
}