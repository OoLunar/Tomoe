namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using System.Threading.Tasks;

    public partial class Config : BaseCommandModule
    {
        [Command("auto_dehoist")]
        public async Task AutoDehoist(CommandContext context)
        {
            bool isEnabled = (bool)Api.Moderation.Config.Get(context.Guild.Id, Api.Moderation.Config.ConfigSetting.AutoDehoist);
            await Program.SendMessage(context, $"AutoDehoist => {isEnabled}. Hoisted nicknames are {(isEnabled ? $"renamed to {Formatter.InlineCode("dehoisted")}" : "kept")}.");
        }

        [Command("auto_dehoist"), Aliases("dehoist"), RequireUserPermissions(Permissions.ManageNicknames), Description("Determines whether nicknames are allowed at the top of the list or not.")]
        public async Task AutoDehoist(CommandContext context, bool isEnabled)
        {
            await Api.Moderation.Config.Set(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.AutoDehoist, isEnabled);
            await Program.SendMessage(context, $"Hoisted nicknames will now be {(isEnabled ? $"renamed to {Formatter.InlineCode("dehoisted")}" : "kept")}.");
        }
    }
}
