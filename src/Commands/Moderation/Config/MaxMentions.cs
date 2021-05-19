namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using Humanizer;
    using System.Threading.Tasks;

    public partial class Config : BaseCommandModule
    {
        [Command("max_mentions")]
        public async Task MaxMentions(CommandContext context)
        {
            int maxMentionCount = (int)Api.Moderation.Config.Get(context.Guild.Id, Api.Moderation.Config.ConfigSetting.MaxMentions);
            await Program.SendMessage(context, $"Max Unique Mentions Per Message => {maxMentionCount}. Messages with more than {maxMentionCount} will be removed.");
        }

        [Command("max_mentions"), RequireUserPermissions(Permissions.ManageMessages), Description("Sets the maximum mentions allowed in a message. Unique user pings and unique role pings are added together for the total ping count, which determines if the user gets a strike or not.")]
        public async Task MaxMentions(CommandContext context, [Description("The maximum amount of unique user pings and unique role pings allowed in a message.")] int maxMentionCount)
        {
            await Api.Moderation.Config.Set(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.MaxLines, maxMentionCount);
            await Program.SendMessage(context, $"The maximum mentions allowed in a message is now {maxMentionCount.ToMetric()}.");
        }
    }
}
