namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    using Tomoe.Db;
    using static Tomoe.Commands.Moderation.ModLogs;

    public partial class Config : BaseCommandModule
    {
        [Command("max_mentions")]
        public async Task MaxMentions(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            await Program.SendMessage(context, $"Max Unique Mentions Per Message => {guildConfig.MaxUniqueMentionsPerMessage}. Messages with more than {guildConfig.MaxLinesPerMessage} will be removed.");
        }

        [Command("max_mentions"), RequireUserPermissions(Permissions.ManageMessages), Description("Sets the maximum mentions allowed in a message. Unique user pings and unique role pings are added together for the total ping count, which determines if the user gets a strike or not.")]
        public async Task MaxMentions(CommandContext context, [Description("The maximum amount of unique user pings and unique role pings allowed in a message.")] int maxMentionCount)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.MaxUniqueMentionsPerMessage = maxMentionCount;
            await Record(context.Guild, LogType.ConfigChange, Database, $"MaxMentions => {context.User.Mention} has changed the max mentions count to {guildConfig.MaxUniqueMentionsPerMessage}");
            await Database.SaveChangesAsync();
            await Program.SendMessage(context, $"The maximum mentions allowed in a message is now {maxMentionCount}.");
        }
    }
}
