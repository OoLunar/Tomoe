using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using EdgeDB;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Database.Converters;
using OoLunar.Tomoe.Interfaces;
using OoLunar.Tomoe.Utilities;

namespace OoLunar.Tomoe.Database.Models
{
    /// <summary>
    /// Allows democracy to take place.
    /// </summary>
    [EdgeDBType("Poll")]
    public sealed partial class PollModel : DatabaseTrackable<PollModel>, IExpirable<PollModel>
    {
        /// <summary>
        /// Who created the poll.
        /// </summary>
        //[EdgeDBTypeConverter(typeof(UlongTypeConverter))]
        public ulong CreatorId { get; private set; }

        /// <summary>
        /// Which guild the poll was sent in, if any.
        /// </summary>
        //[EdgeDBTypeConverter(typeof(UlongTypeConverter))]
        public ulong? GuildId { get; private set; }

        /// <summary>
        /// Which channel the poll was sent in.
        /// </summary>
        //[EdgeDBTypeConverter(typeof(UlongTypeConverter))]
        public ulong ChannelId { get; private set; }

        /// <summary>
        /// The id of the poll.
        /// </summary>
        //[EdgeDBTypeConverter(typeof(UlongTypeConverter))]
        public ulong? MessageId { get; private set; }

        /// <summary>
        /// The poll's question.
        /// </summary>
        public string Question { get; private set; } = null!;

        /// <summary>
        /// When the poll ends.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMinutes(5);

        /// <summary>
        /// The available options a user can vote for.
        /// </summary>
        [EdgeDBIgnore]
        public IReadOnlyList<PollOptionModel> Options => _options.ToArray();
        //[EdgeDBTypeConverter(typeof(IEnumerableTypeConverter<PollOptionModel, PollOptionModel>))]
        private ConcurrentHashSet<PollOptionModel> _options { get; set; } = new();

        /// <summary>
        /// The options the user has voted for.
        /// </summary>
        [EdgeDBIgnore]
        public IReadOnlyList<PollVoteModel> Votes => _votes.ToArray();
        //[EdgeDBTypeConverter(typeof(IEnumerableTypeConverter<PollVoteModel, PollVoteModel>))]
        private ConcurrentHashSet<PollVoteModel> _votes { get; set; } = new();

        private ILogger<PollModel> Logger { get; init; } = null!;

        public PollModel() { }
        public PollModel(ILogger<PollModel> logger, ulong creatorId, ulong? guildId, ulong channelId, ulong? messageId, string question, DateTimeOffset expiresAt, IEnumerable<string> options)
        {
            ArgumentNullException.ThrowIfNull(logger);
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException("The question cannot be null, empty, or whitespace.", nameof(question));
            }
            ArgumentNullException.ThrowIfNull(expiresAt);
            if (options == null || !options.Any())
            {
                throw new ArgumentException("Must have at least one option.", nameof(options));
            }

            Logger = logger;
            CreatorId = creatorId;
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = messageId;
            Question = question;
            ExpiresAt = expiresAt;
            _options = new ConcurrentHashSet<PollOptionModel>(options.Select(option => new PollOptionModel(option, this)));
        }

        /// <summary>
        /// Adds a vote to the poll.
        /// </summary>
        /// <param name="userId">Who is adding their vote.</param>
        /// <param name="optionId">Which option their voting for.</param>
        public void AddVote(ulong userId, Guid optionId)
        {
            if (_votes.Any(vote => vote.VoterId == userId))
            {
                throw new InvalidOperationException("User has already voted.");
            }

            _votes.Add(new PollVoteModel(this, userId, _options.First(option => option.Id == optionId)));
        }

        /// <summary>
        /// Removes a user's vote from the poll.
        /// </summary>
        /// <param name="userId">The id of the user's vote to remove.</param>
        /// <returns>Whether a vote was found. Can be used to determine if the user had already voted.</returns>
        public bool RemoveVote(ulong userId)
        {
            PollVoteModel vote = _votes.FirstOrDefault(vote => vote.VoterId == userId) ?? throw new InvalidOperationException("User has not voted."); ;
            return _votes.TryRemove(vote);
        }

        /// <summary>
        /// Which message the poll is attached to. Can only be set once.
        /// </summary>
        /// <param name="messageId">The id of the Discord message.</param>
        public void SetMessageId(ulong messageId)
        {
            // Make sure the message can only be set once.
            if (MessageId != null)
            {
                throw new InvalidOperationException("Message id has already been set.");
            }
            MessageId = messageId;
        }

        /// <summary>
        /// Expires the poll, removing it from the database and informing the community of the results.
        /// </summary>
        /// <param name="serviceProvider">A service provider that allows for grabbing of the required services.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the network operation.</param>
        /// <returns>Whether the poll should be removed from the database or not.</returns>
        public async Task<bool> ExpireAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            if (MessageId == null)
            {
                Logger.LogError("Expired poll {Id} has no message id.", Id);
                return true;
            }

            DiscordShardedClient shardedClient = serviceProvider.GetRequiredService<DiscordShardedClient>();
            DSharpPlus.Entities.Optional<DiscordMessage?> message = await shardedClient.GetMessageAsync(ChannelId, MessageId.Value, GuildId);
            if (!message.HasValue)
            {
                // Message, Channel, Guild was deleted or permissions changed so that we can no longer access it.
                Logger.LogDebug("Poll {Id} (created by {CreatorId} in {GuildId}) is inaccessible due to the message, channel or guild being deleted or permissions were changed. Removing.", Id, CreatorId, GuildId);
                return true;
            }
            else if (message.Value == null)
            {
                // Server problem, post pone 5 minutes.
                Logger.LogDebug("Poll {Id} (created by {CreatorId} in {GuildId}) is inaccessible due to a server problem. Postponing for 5 minutes later.", Id, CreatorId, GuildId);
                ExpiresAt = ExpiresAt.AddMinutes(5);
                return true;
            }

            // We group all the votes together by their option
            // Then we group all the options together by their vote count
            // Order it by their vote count
            // Then we take the first one (the one with the most votes)
            // And now we have the option(s) that either won or tied in votes.
            IEnumerable<IGrouping<PollOptionModel, PollVoteModel>> voteOptions = _votes.GroupBy(vote => vote.Option).GroupBy(group => group.Count()).OrderBy(group => group.Key).First();
            long totalVoteCount = _votes.Count;
            int? winningVoteCount = voteOptions.First().Count();
            string winner = winningVoteCount switch
            {
                null or 0 => "The winner is... Nobody! There weren't any votes...",
                1 => $"The winner is {voteOptions.First().Key.Option} with {winningVoteCount:N0} vote{(winningVoteCount == 1 ? null : "s")}!", // The winner is Minecraft with 14,012 votes!
                2 => $"We have a two way tie between {voteOptions.First().Key.Option} and {voteOptions.ElementAt(1).Key.Option}. Both have {winningVoteCount:N0} vote{(winningVoteCount == 1 ? null : "s")}!", // We have a two way tie between Minecraft and Terraria. Both have 1 vote!
                _ => $"We have a {voteOptions.Count().ToWords()} way tie, each with {winningVoteCount:N0} vote{(winningVoteCount == 1 ? null : "s")}! Nobody could decide between {voteOptions.Select(x => x.Key.Option).Humanize()}." // We have a six way tie, each with 14,012 votes! Nobody could decide between Minecraft, Terraria, Hollow Knight, Mario Kart Wii, Wii Sports and Smash Bros.!
            };

            // Indexing the embeds is safe here because supressing embeds is actually a flag, not a message modification.
            DiscordEmbedBuilder embedBuilder = new(message.Value.Embeds[0])
            {
                Title = "Poll Expired!"
            };
            embedBuilder.AddField("Winner", winner, true);
            DiscordMessageBuilder pollMessageBuilder = new DiscordMessageBuilder().WithEmbed(embedBuilder.Build()).AddComponents(message.Value.Components.Select(component => new DiscordActionRowComponent(component.Components.Select(x => ((DiscordButtonComponent)x).Disable()))));

            DiscordMessageBuilder messageBuilder = new();
            messageBuilder.WithReply(MessageId);
            messageBuilder.WithContent(winner);
            messageBuilder.WithAllowedMentions(Mentions.None);

            try
            {
                // Disable people from voting again by disabling all the buttons.
                await message.Value.ModifyAsync(pollMessageBuilder);

                // Send who won the poll.
                await message.Value.Channel.SendMessageAsync(messageBuilder);
            }
            catch (DiscordException error) when (error.WebResponse.ResponseCode >= 500)
            {
                // The guild is unavailable or Discord threw a 500 error.
                ExpiresAt = ExpiresAt.AddMinutes(5);
                return false;
            }
            catch (DiscordException error)
            {
                Logger.LogError(error, "Failed to expire poll.");
            }

            return true;
        }
    }
}
