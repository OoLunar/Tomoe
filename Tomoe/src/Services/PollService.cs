using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Services
{
    /// <summary>
    /// A service that handles polls, keeping them optimistically in cache and always in the database until they expire.
    /// </summary>
    public sealed class PollService
    {
        /// <summary>
        /// The time to cache polls for.
        /// </summary>
        private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(1);

        /// <summary>
        /// The logger to log to.
        /// </summary>
        private readonly ILogger<PollService> _logger;

        /// <summary>
        /// The cache to store polls in. They're removed from the cache within a minute without being accessed.
        /// </summary>
        private readonly MemoryCacheService _cache;

        /// <summary>
        /// The service to handle expirable objects.
        /// </summary>
        private readonly ExpirableService<PollModel> _expirableService;

        /// <summary>
        /// Creates a new <see cref="PollService"/>.
        /// </summary>
        /// <param name="logger">The logger to log to.</param>
        /// <param name="expirableService">The service to handle expirable objects.</param>
        public PollService(ILogger<PollService> logger, ExpirableService<PollModel> expirableService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _expirableService = expirableService ?? throw new ArgumentNullException(nameof(expirableService));
            _cache = new();
        }

        /// <summary>
        /// Creates a new poll, adding it to the cache and database.
        /// </summary>
        /// <param name="pollId">The ID of the poll to create.</param>
        /// <param name="question">The question of the poll.</param>
        /// <param name="options">The options of the poll.</param>
        /// <param name="expiresAt">The time the poll expires.</param>
        /// <param name="guildId">The ID of the guild the poll was created in.</param>
        /// <param name="channelId">The ID of the channel the poll was created in.</param>
        /// <param name="messageId">The ID of the message the poll was created in.</param>
        /// <returns>The created poll.</returns>
        public async Task<PollModel> CreatePollAsync(Guid pollId, string question, IEnumerable<string> options, DateTime expiresAt, ulong? guildId, ulong channelId, ulong messageId)
        {
            PollModel poll = new(pollId, question, options, expiresAt, guildId, channelId, messageId);
            await _expirableService.AddAsync(poll);
            _cache.Set(pollId, poll, CacheExpiration(poll.ExpiresAt));
            return poll;
        }

        /// <summary>
        /// Gets a poll from the cache or database, returning null if it doesn't exist.
        /// </summary>
        /// <param name="pollId">The ID of the poll to get.</param>
        /// <returns>The poll, or null if it doesn't exist.</returns>
        public async Task<PollModel?> GetPollAsync(Guid pollId)
        {
            // If the poll is in the cache, return it.
            if (_cache.TryGetValue(pollId, out PollModel? poll) && poll is not null)
            {
                // Be sure to update the cache expiration.
                _cache.Set(pollId, poll, CacheExpiration(poll.ExpiresAt));
                return poll;
            }

            // If the poll isn't in the cache, get it from the database.
            poll = await _expirableService.GetAsync(pollId);
            if (poll is not null)
            {
                // Add it to the cache if it exists.
                _cache.Set(pollId, poll, CacheExpiration(poll.ExpiresAt));
            }

            return poll;
        }

        /// <summary>
        /// Removes a poll from the cache and database, returning the poll if it existed.
        /// </summary>
        /// <param name="pollId">The ID of the poll to remove.</param>
        /// <returns>The poll, or null if it didn't exist.</returns>
        public async Task<PollModel?> RemovePollAsync(Guid pollId)
        {
            // Try to grab the poll from the database.
            PollModel? poll = await GetPollAsync(pollId);
            if (poll is not null)
            {
                // Remove it from the cache and database.
                await _expirableService.RemoveAsync(pollId);
                _cache.TryRemove(pollId, out _);
            }

            return poll;
        }

        /// <summary>
        /// Sets the vote of a user for a poll.
        /// </summary>
        /// <param name="pollId">The ID of the poll to set the vote for.</param>
        /// <param name="userId">The ID of the user to set the vote for.</param>
        /// <param name="option">The option to set the vote for.</param>
        public async Task SetVoteAsync(Guid pollId, ulong userId, int option)
        {
            PollModel? poll = await GetPollAsync(pollId);
            if (poll is null)
            {
                _logger.LogWarning("Poll {PollId} does not exist. User {UserId} attempted to vote for option {Option}", pollId, userId, option);
                return;
            }

            poll.Votes[userId] = option;

            // Update the poll in the database and cache.
            await _expirableService.UpdateAsync(poll);
            _cache.Set(pollId, poll, CacheExpiration(poll.ExpiresAt));
        }

        /// <summary>
        /// Removes a user's vote from a poll.
        /// </summary>
        /// <param name="pollId">The ID of the poll to remove the vote from.</param>
        /// <param name="userId">The ID of the user to remove the vote from.</param>
        public async Task<bool> RemoveVoteAsync(Guid pollId, ulong userId)
        {
            PollModel? poll = await GetPollAsync(pollId);
            if (poll is null)
            {
                _logger.LogWarning("Poll {PollId} does not exist. User {UserId} attempted to remove their vote.", pollId, userId);
                return false;
            }
            else if (poll.Votes.Remove(userId))
            {
                // Update the poll in the database and cache.
                await _expirableService.UpdateAsync(poll);
                _cache.Set(pollId, poll, CacheExpiration(poll.ExpiresAt));
                return true;
            }

            // The user didn't have a vote.
            return false;
        }

        /// <summary>
        /// Gets the total number of votes for a poll.
        /// </summary>
        /// <param name="pollId">The ID of the poll to get the total votes for.</param>
        /// <returns>The total number of votes for the poll. Returns 0 if the poll doesn't exist.</returns>
        public async Task<int> GetTotalVotesAsync(Guid pollId) => (await GetPollAsync(pollId))?.Votes.Count ?? 0;

        /// <summary>
        /// Determines the expiration time for a poll in the cache. If the poll expires in less than a minute, it will expire in the cache when the poll is set to expire.
        /// </summary>
        /// <param name="expiresAt">The time the poll expires.</param>
        /// <returns>The time the poll should expire in the cache.</returns>
        private static DateTimeOffset CacheExpiration(DateTimeOffset expiresAt)
        {
            DateTimeOffset expiresInCache = DateTimeOffset.UtcNow.Add(CacheExpirationTime);
            return expiresAt > expiresInCache ? expiresInCache : expiresAt;
        }
    }
}
