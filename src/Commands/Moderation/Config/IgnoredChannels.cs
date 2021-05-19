namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class Config : BaseCommandModule
    {
        [Command("ignored_channels"), Aliases("hidden_channels", "channel_ignored", "ignore_channel", "hide_channel", "channel_ignore"), Description("Shows all ignored channels.")]
        public async Task ChannelIgnored(CommandContext context) => await Program.SendMessage(context, $"Ignored Channels => {string.Join(", ", ((List<ulong>)Api.Moderation.Config.Get(context.Guild.Id, Api.Moderation.Config.ConfigSetting.IgnoredChannels)).Select(channelId => $"<#{channelId}>").DefaultIfEmpty("None set"))}\nTomoe will not respond when commands are used in this channel.");

        [Command("ignore_channel"), Aliases("hide_channel", "channel_ignore"), RequireUserPermissions(Permissions.ManageChannels), Description("Prevents the bot from reading messages and executing commands in the specified channel.")]
        public async Task ChannelIgnore(CommandContext context, [Description("The Discord channel to ignore.")] DiscordChannel discordChannel)
        {
            await Api.Moderation.Config.AddList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.IgnoredChannels, discordChannel.Id);
            await Program.SendMessage(context, $"Channel {discordChannel.Mention} is now ignored! No commands, automod or anything of the sort can be used in that channel!");
        }

        [Command("unignore_channel"), Aliases("show_channel", "channel_unignore"), RequireUserPermissions(Permissions.ManageChannels), Description("Allows the bot to see messages and execute commands in the specified channel.")]
        public async Task ChannelUnignore(CommandContext context, [Description("The Discord invite to unignore.")] DiscordChannel discordChannel)
        {
            if (await Api.Moderation.Config.RemoveList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.IgnoredChannels, discordChannel.Id))
            {
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
            await Api.Moderation.Config.ClearList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.IgnoredChannels);
            await Program.SendMessage(context, "All ignored channels have been cleared from the channel ignore list!");
        }
    }
}



