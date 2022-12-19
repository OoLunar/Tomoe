using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Services
{
    public sealed class PollService
    {
        private readonly MemoryCache _cache;
        private readonly ILogger<PollService> _logger;
        private readonly TimeSpan _pollSaveDuration;
        private readonly ExpirableService<PollModel> _expirableService;

        public PollService(IConfiguration configuration, ILogger<PollService> logger, ExpirableService<PollModel> expirableService)
        {
            _pollSaveDuration = TimeSpan.FromMinutes(configuration.GetValue("poll:save_duration", 1));
            _logger = logger;
            _cache = new(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromSeconds(30) });
            _expirableService = expirableService;
        }

        public async Task<PollModel> CreatePollAsync(Guid pollId, string question, IEnumerable<string> options, DateTime expiresAt, ulong? guildId, ulong channelId, ulong messageId)
        {
            PollModel poll = new(pollId, question, options, expiresAt, guildId, channelId, messageId);
            await _expirableService.AddAsync(poll);
            _cache.Set(pollId, poll, CreateCancellationChangeToken(poll));
            return poll;
        }

        public async Task<PollModel?> GetPollAsync(Guid pollId)
        {
            if (_cache.TryGetValue(pollId, out PollModel? poll) && poll is not null)
            {
                _cache.Remove(pollId);
                _cache.Set(pollId, poll, CreateCancellationChangeToken(poll)); // Reset the timeout everytime it's accessed.
                return poll;
            }

            poll = await _expirableService.GetAsync(pollId);
            if (poll is not null)
            {
                _cache.Set(pollId, poll, CreateCancellationChangeToken(poll));
            }

            return poll;
        }

        public async Task<PollModel?> RemovePollAsync(Guid pollId)
        {
            PollModel? poll = await GetPollAsync(pollId);
            if (poll is not null)
            {
                _cache.Remove(pollId);
                await _expirableService.RemoveAsync(pollId);
            }

            return poll;
        }

        public async Task SetVoteAsync(Guid pollId, ulong userId, int option)
        {
            PollModel? poll = await GetPollAsync(pollId);
            if (poll is null)
            {
                _logger.LogWarning("Poll {PollId} does not exist. User {UserId} attempted to vote for option {Option}", pollId, userId, option);
                return;
            }

            poll.Votes[userId] = option;
            await _expirableService.UpdateAsync(poll);
            _cache.Remove(pollId);
            _cache.Set(pollId, poll, CreateCancellationChangeToken(poll));
        }

        public async Task<bool> RemoveVoteAsync(Guid pollId, ulong userId)
        {
            PollModel? poll = await GetPollAsync(pollId);
            if (poll is null)
            {
                _logger.LogWarning("Poll {PollId} does not exist. User {UserId} attempted to remove their vote.", pollId, userId);
                return false;
            }
            else if (!poll.Votes.ContainsKey(userId))
            {
                return false;
            }

            poll.Votes.Remove(userId);
            await _expirableService.UpdateAsync(poll);
            _cache.Remove(pollId);
            _cache.Set(pollId, poll, CreateCancellationChangeToken(poll));
            return true;
        }

        public async Task<int> GetTotalVotesAsync(Guid pollId) => (await GetPollAsync(pollId))?.Votes.Count ?? 0;

        private CancellationChangeToken CreateCancellationChangeToken(PollModel poll)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            CancellationChangeToken cct = new(new CancellationTokenSource(poll.ExpiresAt < now.Add(_pollSaveDuration) ? poll.ExpiresAt - now : _pollSaveDuration).Token);
            cct.RegisterChangeCallback(pollObject =>
            {
                PollModel poll = (PollModel)pollObject!;
                _logger.LogInformation("Poll {PollId} has expired from the cache.", poll.Id);
            }, poll);
            return cct;
        }
    }
}
