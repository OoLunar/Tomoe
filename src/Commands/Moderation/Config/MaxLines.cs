namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using Humanizer;
    using System.Threading.Tasks;

    public partial class Config : BaseCommandModule
    {
        [Command("max_lines")]
        public async Task MaxLines(CommandContext context)
        {
            int maxLineCount = (int)Api.Moderation.Config.Get(context.Guild.Id, Api.Moderation.Config.ConfigSetting.MaxLines);
            await Program.SendMessage(context, $"Max Lines Per Message => {maxLineCount}. Messages with more than {maxLineCount} will be removed.");
        }

        [Command("max_lines"), Aliases("max_line"), RequireUserPermissions(Permissions.ManageMessages), Description("Sets the limit on the max amount of lines allowed on messages.")]
        public async Task MaxLines(CommandContext context, [Description("The maximum amount of lines allowed in a message.")] int maxLineCount)
        {
            await Api.Moderation.Config.Set(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.MaxLines, maxLineCount);
            await Program.SendMessage(context, $"The maximum lines allowed in a message is now {maxLineCount.ToMetric()}.");
        }
    }
}
