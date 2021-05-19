namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Humanizer;
    using System.Threading.Tasks;

    public class Logging : BaseCommandModule
    {
        [Command("logging"), RequireGuild, RequireUserPermissions(Permissions.ManageChannels), Aliases("logs"), Description("Sets which logs go into what channel.")]
        public async Task ByUser(CommandContext context, Api.Moderation.LogType logType, DiscordChannel channel, bool isEnabled = false)
        {
            await Api.Moderation.Logging.Set(context.Client, context.Guild.Id, context.User.Id, logType, channel, isEnabled);
            await Program.SendMessage(context, $"Event {logType.Humanize()} will now be recorded in channel {channel.Mention}");
        }
    }
}
