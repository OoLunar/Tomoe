using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tomoe.Enums;
using Tomoe.Models;

namespace Tomoe.Commands.Server
{
    [Group("autoreact"), Description("Manages guild auto reactions.")]
    public class AutoReact : BaseCommandModule
    {
        public ILogger<AutoReact> Logger { private get; init; } = null!;
        public DatabaseContext Database { private get; init; } = null!;

        [Command("add"), Description("Adds an auto reaction to a channel.")]
        public async Task AddAsync(CommandContext context, [Description("Which channel to auto-react in.")] DiscordChannel channel, [Description("Which emojis to react with.")] params DiscordEmoji[] emojis) => await AddAsync(context, channel, FilterType.AllMessages, null, emojis);

        [Command("add"), Description("Adds an auto reaction to a channel."),]
        public async Task AddAsync(CommandContext context, [Description("Which channel to auto-react in.")] DiscordChannel channel, [Description("Filter out messages that shouldn't be reacted to.")] FilterType filterType, [Description("An optional argument that may be required when using certain kinds of filter types.")] string? filterTypeInput = null, [Description("Which emojis to react with.")] params DiscordEmoji[] emojis)
        {
            if (emojis.Length == 0)
            {
                await context.RespondAsync("Error: You must provide at least one emoji.");
                return;
            }

            Regex? regex;
            try
            {
#pragma warning disable IDE0072 // Populate switch. Compiler bug: https://github.com/dotnet/roslyn/issues/58468
                regex = filterType switch
                {
                    FilterType.Regex => new(filterTypeInput ?? throw new ArgumentNullException(nameof(filterTypeInput))),
                    FilterType.Phrase => new("^" + Regex.Escape(filterTypeInput ?? throw new ArgumentNullException(nameof(filterTypeInput))) + "$"),
                    FilterType.Ping => new(@"\<@&?!?\d+\>"),
                    FilterType.RolePing => new(@"\<@&\d+\>"),
                    FilterType.UserPing => new(@"\<@!?\d+\>"),
                    _ => null
                };
#pragma warning restore IDE0072 // Populate switch. Compiler bug: https://github.com/dotnet/roslyn/issues/58468
            }
            catch (ArgumentNullException)
            {
                await context.RespondAsync($"Error: You must provide a parameter for the {filterType.Humanize()} filter type.");
                return;
            }

            AutoReactionModel autoReaction = new()
            {
                GuildId = context.Guild.Id,
                ChannelId = context.Channel.Id,
                EmojiData = emojis.Select(emoji => new EmojiData(emoji)).ToArray(),
                FilterType = filterType,
                Regex = regex
            };

            if (Database.AutoReactions.Any(x => x.GuildId == context.Guild.Id && x.ChannelId == context.Channel.Id && x.FilterType == filterType && x.Regex == regex && x.EmojiData.SequenceEqual(autoReaction.EmojiData)))
            {
                await context.RespondAsync("Error: This reaction already exists.");
                return;
            }

            Database.AutoReactions.Add(autoReaction);
            await Database.SaveChangesAsync();

            await context.RespondAsync($"Successfully added auto reaction.");
        }

        [Command("remove"), Description("Removes an auto reaction from a channel.")]
        public async Task RemoveAsync(CommandContext context, [Description("Which auto reactions to remove. Ids can be grabbed from the list command.")] params Guid[] ids)
        {
            IEnumerable<AutoReactionModel>? autoReaction = Database.AutoReactions.Where(databaseAutoReaction => ids.Contains(databaseAutoReaction.Id));
            if (autoReaction == null)
            {
                await context.RespondAsync("Error: No auto reaction with that ID.");
                return;
            }

            int autoReactionCount = autoReaction.Count();
            Database.AutoReactions.RemoveRange(autoReaction);

            await context.RespondAsync($"Removed {autoReactionCount} auto reaction{(autoReactionCount == 1 ? "" : "s")}.");
        }

        [Command("list"), Description("Lists all auto reactions in a channel.")]
        public async Task ListAsync(CommandContext context, [Description("Optionally list specific auto reactions for more information on them.")] params Guid[] ids)
        {
            List<Page> embeds = new();
            foreach (AutoReactionModel autoReaction in Database.AutoReactions.Where(databaseAutoReaction => ids.Contains(databaseAutoReaction.Id)))
            {
                DiscordEmbedBuilder embedBuilder = new();
                embedBuilder.Title = $"Auto Reaction #{autoReaction.Id}";
                embedBuilder.AddField("Channel", autoReaction.ChannelId.ToString(CultureInfo.InvariantCulture));
                embedBuilder.AddField("Filter", autoReaction.FilterType.Humanize());
                embedBuilder.AddField("Reaction", string.Join(" ", autoReaction.EmojiData.Select(emojiData => emojiData.EmojiId == 0 ? DiscordEmoji.FromUnicode(emojiData.EmojiName) : DiscordEmoji.FromGuildEmote(context.Client, emojiData.EmojiId))));
                if (autoReaction.Regex != null)
                {
                    embedBuilder.AddField("Regex", autoReaction.Regex?.ToString() ?? "None");
                }
                embeds.Add(new Page("", embedBuilder));
            }

            InteractivityExtension? interactivity = context.Client.GetInteractivity();
            if (embeds.Count == 0)
            {
                await context.RespondAsync("No auto reactions found.");
                return;
            }
            await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, embeds);
        }

        [Command("list"), Description("Lists all auto reactions in a channel.")]
        public async Task ListAsync(CommandContext context, [Description("List all auto reactions which match this filter.")] FilterType filterType)
        {
            List<Page> embeds = new();
            foreach (AutoReactionModel autoReaction in Database.AutoReactions.Where(databaseAutoReaction => databaseAutoReaction.FilterType == filterType && databaseAutoReaction.GuildId == context.Guild.Id))
            {
                DiscordEmbedBuilder embedBuilder = new();
                embedBuilder.Title = $"Auto Reaction #{autoReaction.Id}";
                embedBuilder.AddField("Channel", autoReaction.ChannelId.ToString(CultureInfo.InvariantCulture));
                embedBuilder.AddField("Filter", autoReaction.FilterType.Humanize());
                embedBuilder.AddField("Reaction", string.Join(" ", autoReaction.EmojiData.Select(emojiData => emojiData.EmojiId == 0 ? DiscordEmoji.FromUnicode(emojiData.EmojiName) : DiscordEmoji.FromGuildEmote(context.Client, emojiData.EmojiId))));
                if (autoReaction.Regex != null)
                {
                    embedBuilder.AddField("Regex", autoReaction.Regex?.ToString() ?? "None");
                }
                embeds.Add(new Page("", embedBuilder));
            }

            InteractivityExtension? interactivity = context.Client.GetInteractivity();
            if (embeds.Count == 0)
            {

            }
            await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, embeds);
        }

        [Command("list"), Description("Lists all auto reactions in a channel.")]
        public async Task ListAsync(CommandContext context, [Description("List all auto reactions that use the requested emojis.")] params DiscordEmoji[] emojis)
        {
            List<Page> embeds = new();

            IEnumerable<string> dynamicEmojis = emojis.Select(emoji => emoji.Id == 0 ? emoji.Name : emoji.Id.ToString(CultureInfo.InvariantCulture));
            foreach (AutoReactionModel autoReaction in Database.AutoReactions.Where(databaseAutoReaction => databaseAutoReaction.GuildId == context.Guild.Id && databaseAutoReaction.EmojiData.Any(emojiData => dynamicEmojis.Contains(emojiData.EmojiName) || dynamicEmojis.Contains(emojiData.EmojiId.ToString(CultureInfo.InvariantCulture)))))
            {
                DiscordEmbedBuilder embedBuilder = new();
                embedBuilder.Title = $"Auto Reaction #{autoReaction.Id}";
                embedBuilder.AddField("Channel", autoReaction.ChannelId.ToString(CultureInfo.InvariantCulture));
                embedBuilder.AddField("Filter", autoReaction.FilterType.Humanize());
                embedBuilder.AddField("Reaction", string.Join(" ", autoReaction.EmojiData.Select(emojiData => emojiData.EmojiId == 0 ? DiscordEmoji.FromUnicode(emojiData.EmojiName) : DiscordEmoji.FromGuildEmote(context.Client, emojiData.EmojiId))));
                if (autoReaction.Regex != null)
                {
                    embedBuilder.AddField("Regex", autoReaction.Regex?.ToString() ?? "None");
                }
                embeds.Add(new Page("", embedBuilder));
            }

            InteractivityExtension? interactivity = context.Client.GetInteractivity();
            if (embeds.Count == 0)
            {

            }
            await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, embeds);
        }

        [Command("list"), Description("Lists all auto reactions in a channel.")]
        public async Task ListAsync(CommandContext context, [Description("List all of the requested channel's auto reactions.")] DiscordChannel channel)
        {
            List<Page> embeds = new();
            foreach (AutoReactionModel autoReaction in Database.AutoReactions.Where(databaseAutoReaction => databaseAutoReaction.ChannelId == channel.Id && databaseAutoReaction.GuildId == context.Guild.Id))
            {
                DiscordEmbedBuilder embedBuilder = new();
                embedBuilder.Title = $"Auto Reaction #{autoReaction.Id}";
                embedBuilder.AddField("Channel", autoReaction.ChannelId.ToString(CultureInfo.InvariantCulture));
                embedBuilder.AddField("Filter", autoReaction.FilterType.Humanize());
                embedBuilder.AddField("Reaction", string.Join(" ", autoReaction.EmojiData.Select(emojiData => emojiData.EmojiId == 0 ? DiscordEmoji.FromUnicode(emojiData.EmojiName) : DiscordEmoji.FromGuildEmote(context.Client, emojiData.EmojiId))));
                if (autoReaction.Regex != null)
                {
                    embedBuilder.AddField("Regex", autoReaction.Regex?.ToString() ?? "None");
                }
                embeds.Add(new Page("", embedBuilder));
            }

            InteractivityExtension? interactivity = context.Client.GetInteractivity();
            if (embeds.Count == 0)
            {

            }
            await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, embeds);
        }
    }
}