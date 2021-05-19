namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using System.Threading.Tasks;

    public partial class Config : BaseCommandModule
    {
        [Command("auto_delete")]
        public async Task DeleteBadMessages(CommandContext context)
        {
            bool isEnabled = (bool)Api.Moderation.Config.Get(context.Guild.Id, Api.Moderation.Config.ConfigSetting.AutoDelete);
            await Program.SendMessage(context, $"AutoDelete => {isEnabled}. Automod will {(isEnabled ? "now" : "not")} delete messages.");
        }

        [Command("auto_delete"), RequireUserPermissions(Permissions.ManageMessages), Description("Determines if messages are removed when automod activates.")]
        public async Task DeleteBadMessages(CommandContext context, bool isEnabled)
        {
            await Api.Moderation.Config.Set(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.AutoStrike, isEnabled);
            await Program.SendMessage(context, $"Automod will {(isEnabled ? "now" : "no longer")} delete messages.");
        }
    }
}

