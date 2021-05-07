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
    using static Tomoe.Commands.Moderation.ModLogs;

    [Group("auto_react"), RequireGuild, Description("Reacts automatically when a new message is posted."), Aliases("autoreact", "ar"), RequireUserPermissions(Permissions.AddReactions), RequireBotPermissions(Permissions.AddReactions)]
    public class AutoReactions : BaseCommandModule
    {
        public Database Database { private get; set; }

        [GroupCommand]
        public async Task Overload(CommandContext context, [Description("The channel to react in.")] DiscordChannel channel, [Description("The emoji to react with.")] DiscordEmoji emoji)
        {
            AutoReaction autoReaction = Database.AutoReactions.FirstOrDefault(autoReaction => autoReaction.GuildId == context.Guild.Id && autoReaction.ChannelId == channel.Id && autoReaction.EmojiName == (emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString()));
            if (autoReaction != null)
            {
                _ = await Program.SendMessage(context, $"Auto reaction {emoji} on {channel.Mention} already exists!");
                return;
            }

            if (channel.Type == ChannelType.Category)
            {
                foreach (DiscordChannel subchannel in channel.Children)
                {
                    autoReaction = new();
                    autoReaction.GuildId = context.Guild.Id;
                    autoReaction.ChannelId = channel.Id;
                    autoReaction.EmojiName = emoji.GetDiscordName();
                    _ = Database.AutoReactions.Add(autoReaction);
                }
                await Record(context.Guild, LogType.AutoReactionCreate, Database, $"{context.User.Mention} has created an autoreaction with emoji {emoji} on all current channels in category {channel.Mention}.");
                _ = await Database.SaveChangesAsync();
                _ = await Program.SendMessage(context, $"From here on out, every message in the current channels of category {channel.Mention} will have the {emoji} reaction added to it!");
            }
            else if (channel.Type is ChannelType.Text or ChannelType.News)
            {
                autoReaction = new();
                autoReaction.GuildId = context.Guild.Id;
                autoReaction.ChannelId = channel.Id;
                autoReaction.EmojiName = emoji.GetDiscordName();
                _ = Database.AutoReactions.Add(autoReaction);
                await Record(context.Guild, LogType.AutoReactionCreate, Database, $"{context.User.Mention} has created an autoreaction with emoji {emoji} on channel {channel.Mention}.");
                _ = await Database.SaveChangesAsync();
                _ = await Program.SendMessage(context, $"From here on out, every message in {channel.Mention} will have the {emoji} reaction added to it!");
            }
            else
            {
                _ = await Program.SendMessage(context, Formatter.Bold($"[Error]: The channel must be text, news or a category!"));
            }
        }

        [Command("list"), Description("Shows all the current auto reactions for a channel."), Aliases("show", "ls")]
        public async Task List(CommandContext context, [Description("Lists all autoreactions for the specified channel.")] DiscordChannel channel)
        {
            StringBuilder stringBuilder = new();
            foreach (AutoReaction autoReaction in Database.AutoReactions.Where(autoReaction => autoReaction.ChannelId == channel.Id && autoReaction.GuildId == context.Guild.Id))
            {
                _ = stringBuilder.AppendLine($"{DiscordEmoji.FromName(context.Client, autoReaction.EmojiName, true)}");
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Autoreactions in channel {channel.Mention}");
            if (stringBuilder.Length <= 2000)
            {
                embedBuilder.Description = stringBuilder.ToString();
                _ = await Program.SendMessage(context, null, embedBuilder.Build());
            }
            else
            {
                InteractivityExtension interactivity = context.Client.GetInteractivity();
                IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(stringBuilder.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line, embedBuilder);
                await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
            }
        }

        [Command("remove"), Description("Removes an autoreaction from a channel."), Aliases("rm", "delete", "del")]
        public async Task Remove(CommandContext context, [Description("The channel to remove the autoreaction from.")] DiscordChannel channel, [Description("The emoji to stop autoreacting with.")] DiscordEmoji emoji = null)
        {
            if (channel.Type == ChannelType.Category)
            {
                List<AutoReaction> autoReactions = emoji == null
                    ? Database.AutoReactions.Where(autoReaction => autoReaction.GuildId == context.Guild.Id && channel.Children.Select(channel => channel.Id).Contains(autoReaction.ChannelId)).ToList()
                    : Database.AutoReactions.Where(autoReaction => autoReaction.GuildId == context.Guild.Id && channel.Children.Select(channel => channel.Id).Contains(autoReaction.ChannelId) && autoReaction.EmojiName == emoji.Name).ToList();

                if (autoReactions.Count == 0)
                {
                    await Program.SendMessage(context, Formatter.Bold($"[Error]: There are no autoreactions on any channels in category {channel.Mention}!"));
                }
                else
                {
                    Database.AutoReactions.RemoveRange(autoReactions);
                    if (emoji == null)
                    {
                        await Record(context.Guild, LogType.AutoReactionDelete, Database, $"{context.User.Mention} has removed all autoreactions on all current channels of category {channel.Mention}.");
                        await Program.SendMessage(context, $"All autoreactions on current channels of category {channel.Mention} have been removed!");
                    }
                    else
                    {
                        await Record(context.Guild, LogType.AutoReactionDelete, Database, $"{context.User.Mention} has removed the {emoji} autoreactions on all current channels of category {channel.Mention}.");
                        await Program.SendMessage(context, $"All {emoji} autoreactions on current channels of category {channel.Mention} have been removed!");
                    }
                }
            }
            else if (channel.Type is ChannelType.Text or ChannelType.News)
            {
                AutoReaction autoReaction = Database.AutoReactions.FirstOrDefault(autoReaction => autoReaction.GuildId == context.Guild.Id && autoReaction.ChannelId == channel.Id && autoReaction.EmojiName == (emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString()));
                if (autoReaction != null)
                {
                    _ = Database.AutoReactions.Remove(autoReaction);
                    await Record(context.Guild, LogType.AutoReactionDelete, Database, $"{context.User.Mention} has removed an autoreaction with emoji {emoji} on channel {channel.Mention}.");
                    _ = await Database.SaveChangesAsync();
                    _ = await Program.SendMessage(context, $"Auto reaction {emoji} on {channel.Mention} has been removed!");
                    return;
                }
                else
                {
                    _ = await Program.SendMessage(context, Formatter.Bold("[Error]: Autoreaction doesn't exist!"));
                }
            }
            else
            {
                _ = await Program.SendMessage(context, Formatter.Bold($"[Error]: The channel must be text, news or a category!"));
            }
        }
    }
}
