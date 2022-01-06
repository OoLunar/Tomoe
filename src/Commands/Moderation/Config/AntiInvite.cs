using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace Tomoe.Commands.Moderation
{
    public partial class Config : BaseCommandModule
    {
        [Command("anti_invite")]
        public async Task AntiInvite(CommandContext context)
        {
            bool isEnabled = (bool)Api.Moderation.Config.Get(context.Guild.Id, Api.Moderation.Config.ConfigSetting.AntiInvite);
            await Program.SendMessage(context, $"AntiInvite => {isEnabled}. Invites are {(isEnabled ? "removed" : "kept")} when posted.");
        }

        [Command("anti_invite"), Aliases("remove_invites"), RequireUserPermissions(Permissions.ManageMessages), Description("Determines whether invites are allowed to be posted or not.")]
        public async Task AntiInvite(CommandContext context, bool isEnabled)
        {
            await Api.Moderation.Config.Set(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.AntiInvite, isEnabled);
            await Program.SendMessage(context, $"Invites will now be {(isEnabled ? "removed" : "kept")} when posted.");
        }
    }
}