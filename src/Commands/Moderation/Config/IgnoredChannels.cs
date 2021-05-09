namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;
    using static Tomoe.Commands.Moderation.ModLogs;

    public partial class Config : BaseCommandModule
    {
        [Command("ignored_channels"), Aliases("hidden_channels", "channel_ignored", "ignore_channel", "hide_channel", "channel_ignore"), Description("Shows all ignored channels.")]
        public async Task ChannelIgnored(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            await Program.SendMessage(context, $"Ignored Channels => {string.Join(", ", guildConfig.IgnoredChannels.Select(channelId => $"<#{channelId}>").DefaultIfEmpty("None set"))}\nTomoe will not respond when commands are used in this channel.");
        }

        [Command("ignore_channel"), Aliases("hide_channel", "channel_ignore"), RequireUserPermissions(Permissions.ManageChannels), Description("Prevents the bot from reading messages and executing commands in the specified channel.")]
        public async Task ChannelIgnore(CommandContext context, [Description("The Discord channel to ignore.")] DiscordChannel discordChannel)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            if (guildConfig.IgnoredChannels.Contains(discordChannel.Id))
            {
                await Program.SendMessage(context, $"Invite discord.gg/{discordChannel.Mention} was already whitelisted!");
            }
            else
            {
                guildConfig.IgnoredChannels.Add(discordChannel.Id);
                Database.Entry(guildConfig).State = EntityState.Modified;
                await Record(context.Guild, LogType.ConfigChange, Database, $"Ignored Channels => {context.User.Mention} has added the channel {discordChannel.Mention} to the channel ignore list.");
                await Database.SaveChangesAsync();
                await Program.SendMessage(context, $"Invite discord.gg/{discordChannel.Mention} is now whitelisted.");
            }
        }

        [Command("unignore_channel"), Aliases("show_channel", "channel_unignore"), RequireUserPermissions(Permissions.ManageChannels), Description("Allows the bot to see messages and execute commands in the specified channel.")]
        public async Task ChannelUnignore(CommandContext context, [Description("The Discord invite to unignore.")] DiscordChannel discordChannel)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            if (guildConfig.IgnoredChannels.Remove(discordChannel.Id))
            {
                Database.Entry(guildConfig).State = EntityState.Modified;
                await Record(context.Guild, LogType.ConfigChange, Database, $"Ignored Channels => {context.User.Mention} has removed the channel {discordChannel.Mention} from the channel ignore list.");
                await Database.SaveChangesAsync();
                await Program.SendMessage(context, "The channel is now shown.");
            }
            else
            {
                await Program.SendMessage(context, "The channel wasn't hidden!");
            }
        }

        [Command("channel_clear"), Aliases("channel_list_clear", "channel_clear_list"), RequireUserPermissions(Permissions.ManageChannels), Description("Removes all ignored channels from the channel list.")]
        public async Task ChannelClearList(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.IgnoredChannels.Clear();
            Database.Entry(guildConfig).State = EntityState.Modified;
            await Record(context.Guild, LogType.ConfigChange, Database, $"Ignored Channels => {context.User.Mention} cleared all ignored channels from the channel ignore list.");
            await Database.SaveChangesAsync();
            await Program.SendMessage(context, "All ignored channels have been cleared from the channel ignore list!");
        }
    }
}



