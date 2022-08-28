using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using EdgeDB;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Interfaces;
using OoLunar.Tomoe.Utilities;

namespace OoLunar.Tomoe.Database
{
    /// <summary>
    /// Allows democracy to take place.
    /// </summary>
    [EdgeDBType("Poll")]
    public sealed partial class PollModel : ICopyable<PollModel>, IExpirable<PollModel>
    {
        /// <summary>
        /// The id of the poll, assigned by the database.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Who created the poll.
        /// </summary>
        public ulong CreatorId { get; private init; }

        /// <summary>
        /// Which guild the poll was sent in, if any.
        /// </summary>
        public ulong? GuildId { get; private init; }

        /// <summary>
        /// Which channel the poll was sent in.
        /// </summary>
        public ulong ChannelId { get; private init; }

        /// <summary>
        /// The id of the poll.
        /// </summary>
        public ulong? MessageId { get; private set; }

        /// <summary>
        /// The poll's question.
        /// </summary>
        public string Question { get; private init; } = null!;

        /// <summary>
        /// When the poll ends.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMinutes(5);

        /// <summary>
        /// The available options a user can vote for.
        /// </summary>
        public IReadOnlyList<PollOptionModel> Options => _options.AsReadOnly();
        private List<PollOptionModel> _options { get; init; } = new();

        /// <summary>
        /// The options the user has voted for.
        /// </summary>
        public IReadOnlyList<PollVoteModel> Votes => _votes.AsReadOnly();
        private List<PollVoteModel> _votes { get; init; } = new();

        private EdgeDBClient EdgeDBClient { get; init; } = null!;
        private ILogger<PollModel> Logger { get; init; } = null!;

        [EdgeDBDeserializer]
        private PollModel(IDictionary<string, object?> raw)
        {
            Id = (Guid)raw["id"]!;
            CreatorId = (ulong)raw["creator_id"]!;
            GuildId = (ulong?)raw["guild_id"];
            ChannelId = (ulong)raw["channel_id"]!;
            MessageId = (ulong)raw["message_id"]!;
            Question = (string)raw["question"]!;
            _options = (List<PollOptionModel>)raw["options"]!;
            _votes = (List<PollVoteModel>)raw["votes"]!;
        }

        [Obsolete("This constructor is only to be used by EdgeDB.", true)]
        public PollModel() { }

        public PollModel(EdgeDBClient edgeDBClient, ILogger<PollModel> logger, ulong creatorId, ulong? guildId, ulong channelId, ulong? messageId, string question, DateTimeOffset expiresAt, IEnumerable<string> options)
        {
            ArgumentNullException.ThrowIfNull(edgeDBClient);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrEmpty(question.Trim(), nameof(question));
            ArgumentNullException.ThrowIfNull(expiresAt);
            if (options == null || !options.Any())
            {
                throw new ArgumentException("Must have at least one option.", nameof(options));
            }

            EdgeDBClient = edgeDBClient;
            Logger = logger;

            CreatorId = creatorId;
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = messageId;
            Question = question;
            ExpiresAt = expiresAt;
            _options = new List<PollOptionModel>(options.Select(option => new PollOptionModel(option, this)));
        }

        [SuppressMessage("Roslyn", "IDE0046", Justification = "Ternary operator rabbit hole.")]
        public async Task<PollModel> AddVoteAsync(ulong userId, Guid optionId, CancellationToken cancellationToken = default)
        {
            if (_votes.Any(v => v.VoterId == userId))
            {
                throw new InvalidOperationException("User has already voted.");
            }
            else if (Id == null)
            {
                throw new InvalidOperationException("Poll has not been saved.");
            }
            else
            {
                return Copy((await EdgeDBClient.QueryAsync<PollModel>(AddVoteQuery, new Dictionary<string, object?>
                {
                    ["poll"] = this,
                    ["userId"] = userId,
                    ["optionId"] = optionId
                }, Capabilities.Modifications, cancellationToken)).FirstOrDefault() ?? throw new InvalidOperationException($"Poll {Id} does not exist in the database."));
            }
        }

        [SuppressMessage("Roslyn", "IDE0046", Justification = "Ternary operator rabbit hole.")]
        public async Task<PollModel> RemoveVoteAsync(ulong userId, CancellationToken cancellationToken = default)
        {
            if (!_votes.Any(v => v.VoterId == userId))
            {
                throw new InvalidOperationException("User has not voted.");
            }
            else if (Id == null)
            {
                throw new InvalidOperationException("Poll has not been saved.");
            }
            else
            {
                return Copy((await EdgeDBClient.QueryAsync<PollModel>(RemoveVoteQuery, new Dictionary<string, object?>
                {
                    ["poll"] = this,
                    ["userId"] = userId
                }, Capabilities.Modifications, cancellationToken)).FirstOrDefault() ?? throw new InvalidOperationException($"Poll {Id} does not exist in the database."));
            }
        }

        public async Task<PollModel> UpdateMessageIdAsync(DiscordMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            if (Id == null)
            {
                MessageId = message.Id;
                return this;
            }
            else
            {
                return Copy((await EdgeDBClient.QueryAsync<PollModel>(UpdateMessageQuery, new Dictionary<string, object?>
                {
                    ["pollId"] = Id
                }, Capabilities.Modifications, cancellationToken)).FirstOrDefault() ?? throw new InvalidOperationException($"Poll {Id} does not exist in the database."));
            }
        }

        public PollModel Copy(PollModel old)
        {
            Id = old.Id;

            _options.Clear();
            _options.AddRange(old._options);

            _votes.Clear();
            _votes.AddRange(old._votes);
            return this;
        }

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
                Logger.LogDebug("Poll {Id} (created by {CreatorId} in {GuildId}) is now inaccessible due to the message, channel or guild being deleted or permissions were changed. Removing.", Id, CreatorId, GuildId);
                return true;
            }
            else if (message.Value == null)
            {
                // Server problem, post pone 5 minutes.
                Logger.LogDebug("Poll {Id} (created by {CreatorId} in {GuildId}) is now inaccessible due to a server problem. Postponing for 5 minutes later.", Id, CreatorId, GuildId);
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

        public void Dispose() => throw new NotImplementedException();
    }
}
