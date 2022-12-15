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
        private readonly ILogger<RoleMenuService> _logger;
        private readonly TimeSpan _pollSaveDuration;
        private readonly ExpirableService<PollModel> _expirableService;

        public PollService(IConfiguration configuration, ILogger<RoleMenuService> logger, ExpirableService<PollModel> expirableService)
        {
            _pollSaveDuration = TimeSpan.FromMinutes(configuration.GetValue("poll:save_duration", 2));
            _logger = logger;
            _cache = new(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromMinutes(2) });
            _expirableService = expirableService;
        }

        public async Task<PollModel> CreatePollAsync(Guid pollId, string question, IEnumerable<string> options, DateTime expiresAt, ulong? guildId, ulong channelId, ulong messageId)
        {
            PollModel poll = new(pollId, question, options, expiresAt, guildId, channelId, messageId);
            await _expirableService.AddAsync(poll);
            _cache.Set(poll.Id, poll, CreateCancellationChangeToken(poll));
            return poll;
        }

        public async Task<PollModel?> GetPollAsync(Guid pollId)
        {
            if (_cache.TryGetValue(pollId, out PollModel? poll))
            {
                _cache.Set(poll!.Id, poll, CreateCancellationChangeToken(poll)); // Reset the timeout everytime it's accessed.
                return poll;
            }

            poll = await _expirableService.GetAsync(pollId);
            if (poll != null)
            {
                _cache.Set(poll.Id, poll, CreateCancellationChangeToken(poll));
            }

            return poll;
        }

        public async Task<PollModel?> RemovePollAsync(Guid pollId)
        {
            PollModel? poll = await GetPollAsync(pollId);
            if (poll != null)
            {
                _cache.Remove(pollId);
                await _expirableService.RemoveAsync(pollId);
            }

            return poll;
        }

        public async Task SetVoteAsync(Guid pollId, ulong userId, int option)
        {
            PollModel poll = (await GetPollAsync(pollId))!;
            poll.Votes[userId] = option;
            await _expirableService.UpdateAsync(poll);
            _cache.Set(poll.Id, poll, CreateCancellationChangeToken(poll));
        }

        public async Task<bool> RemoveVoteAsync(Guid pollId, ulong userId)
        {
            PollModel poll = (await GetPollAsync(pollId))!;
            if (!poll.Votes.ContainsKey(userId))
            {
                return false;
            }

            poll.Votes.Remove(userId);
            await _expirableService.UpdateAsync(poll);
            _cache.Set(poll.Id, poll, CreateCancellationChangeToken(poll));
            return true;
        }

        public async Task<int> GetTotalVotesAsync(Guid pollId) => (await GetPollAsync(pollId))?.Votes.Count ?? 0;

        private CancellationChangeToken CreateCancellationChangeToken(PollModel poll)
        {
            CancellationChangeToken cct = new(new CancellationTokenSource(_pollSaveDuration).Token);
            cct.RegisterChangeCallback(async pollObject =>
            {
                PollModel poll = (PollModel)pollObject!;
                if (poll.ExpiresAt > DateTime.UtcNow)
                {
                    await _expirableService.UpdateAsync(poll);
                }

                _cache.Remove(poll.Id);
                _logger.LogInformation("Poll {PollId} has expired from the cache.", poll.Id);
            }, poll);
            return cct;
        }
    }
}
