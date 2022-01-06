using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace Tomoe.Commands.Moderation
{
    public partial class Config : BaseCommandModule
    {
        [Command("show_error")]
        public async Task ShowPermissionErrors(CommandContext context)
        {
            bool isEnabled = (bool)Api.Moderation.Config.Get(context.Guild.Id, Api.Moderation.Config.ConfigSetting.ShowPermissionErrors);
            await Program.SendMessage(context, $"Show Errors => {isEnabled}. Errors will show in the form of {(isEnabled ? "messages" : "reactions")}.");
        }

        [Command("show_error"), Aliases("show_errors"), RequireUserPermissions(Permissions.ManageMessages), Description("When enabled, commands that fail for any reason will use a reaction instead of a message to show failure.")]
        public async Task ShowPermissionErrors(CommandContext context, bool isEnabled)
        {
            await Api.Moderation.Config.Set(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.ShowPermissionErrors, isEnabled);
            await Program.SendMessage(context, $"Errors will {(isEnabled ? "now" : "no longer")} show when the user doesn't have the correct permissions.");
        }
    }
}