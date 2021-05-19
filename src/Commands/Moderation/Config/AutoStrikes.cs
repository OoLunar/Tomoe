namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using System.Threading.Tasks;

    public partial class Config : BaseCommandModule
    {
        [Command("auto_strike")]
        public async Task StrikeAutomod(CommandContext context)
        {
            bool isEnabled = (bool)Api.Moderation.Config.Get(context.Guild.Id, Api.Moderation.Config.ConfigSetting.AutoStrike);
            await Program.SendMessage(context, $"AutoStrike => {isEnabled}. Automod will {(isEnabled ? "now" : "not")} give out strikes.");
        }

        [Command("auto_strike"), Aliases("auto_strikes"), RequireUserPermissions(Permissions.ManageMessages), Description("Determines whether automod should add a strike to the victim when automodding.")]
        public async Task StrikeAutomod(CommandContext context, bool isEnabled)
        {
            await Api.Moderation.Config.Set(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.AutoStrike, isEnabled);
            await Program.SendMessage(context, $"Automod will {(isEnabled ? "now" : "no longer")} give out strikes.");
        }
    }
}
