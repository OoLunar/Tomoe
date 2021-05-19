namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Extensions;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Tomoe.Db;

    [Group("auto_react"), RequireGuild, Description("Reacts automatically when a new message is posted."), Aliases("autoreact", "ar"), RequireUserPermissions(Permissions.AddReactions | Permissions.ManageChannels), RequireBotPermissions(Permissions.AddReactions)]
    public class AutoReactions : BaseCommandModule
    {
        public Database Database { private get; set; }

        [GroupCommand]
        public async Task Overload(CommandContext context, [Description("The channel to react in.")] DiscordChannel channel, [Description("The emojis to react with.")] params DiscordEmoji[] emojis)
        {
            if (channel.Type == ChannelType.Category)
            {
                foreach (DiscordChannel subchannel in channel.Children)
                {
                    await Api.Moderation.AutoReactions.Create(context.Client, context.Guild.Id, subchannel.Id, context.User.Id, emojis.Select(emoji => emoji.GetDiscordName()).ToArray());
                }
                await Program.SendMessage(context, $"From here on out, every message in the current channels of category {channel.Mention} will have the following autoreactions added to it: {string.Join(", ", emojis.AsEnumerable())}");
            }
            else if (channel.Type is ChannelType.Text or ChannelType.News)
            {

                if (await Api.Moderation.AutoReactions.Create(context.Client, context.Guild.Id, channel.Id, context.User.Id, emojis.Select(emoji => emoji.GetDiscordName()).ToArray()))
                {
                    await Program.SendMessage(context, $"The following autoreactions in channel {channel.Mention} have been created: {string.Join(", ", emojis.AsEnumerable())}");
                }
                else
                {
                    await Program.SendMessage(context, $"Those autoreactions already exist!");
                }
            }
            else
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: The channel must be text, news or a category!"));
            }
        }

        [Command("list"), Description("Shows all the current auto reactions for a channel."), Aliases("show", "ls")]
        public async Task List(CommandContext context, [Description("Lists all autoreactions for the specified channel.")] DiscordChannel channel)
        {
            StringBuilder stringBuilder = new();
            foreach (AutoReaction autoReaction in Api.Moderation.AutoReactions.List(context.Guild.Id, channel.Id))
            {
                stringBuilder.AppendLine($"{DiscordEmoji.FromName(context.Client, autoReaction.EmojiName, true)}");
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Autoreactions in channel {channel.Mention}");
            if (stringBuilder.Length <= 2000)
            {
                embedBuilder.Description = stringBuilder.ToString();
                await Program.SendMessage(context, null, embedBuilder.Build());
            }
            else
            {
                InteractivityExtension interactivity = context.Client.GetInteractivity();
                IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(stringBuilder.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line, embedBuilder);
                await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
            }
        }

        [Command("remove"), Description("Removes an autoreaction from a channel."), Aliases("rm", "delete", "del")]
        public async Task Remove(CommandContext context, [Description("The channel to remove the autoreaction from.")] DiscordChannel channel, [Description("The emoji to stop autoreacting with.")] params DiscordEmoji[] emojis)
        {
            if (channel.Type == ChannelType.Category)
            {
                foreach (DiscordChannel subchannel in channel.Children)
                {
                    await Api.Moderation.AutoReactions.Delete(context.Client, context.Guild.Id, subchannel.Id, context.User.Id, emojis.Select(emoji => emoji.GetDiscordName()).ToArray());
                }
                await Program.SendMessage(context, $"All autoreactions in the channels of category {channel.Mention} with the following emojis have been removed: {string.Join(", ", emojis.AsEnumerable())}");
            }
            else if (channel.Type is ChannelType.Text or ChannelType.News)
            {
                if (await Api.Moderation.AutoReactions.Delete(context.Client, context.Guild.Id, channel.Id, context.User.Id, emojis.Select(emoji => emoji.GetDiscordName()).ToArray()))
                {
                    await Program.SendMessage(context, $"The following autoreactions have been deleted from channel {channel.Mention}: {string.Join(", ", emojis.AsEnumerable())}");
                }
                else
                {
                    await Program.SendMessage(context, $"Those autoreactions already exist!");
                }
            }
            else
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: The channel must be text, news or a category!"));
            }
        }
    }
}
