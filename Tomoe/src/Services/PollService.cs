using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Services
{
    public sealed class PollService
    {
        private readonly MemoryCache _cache;
        private readonly DatabaseContext _databaseContext;
        private readonly ILogger<RoleMenuService> _logger;
        private readonly TimeSpan _pollSaveDuration;

        public PollService(IConfiguration configuration, DatabaseContext databaseContext, ILogger<RoleMenuService> logger)
        {
            _pollSaveDuration = TimeSpan.FromMinutes(configuration.GetValue("poll:save_duration", 2));
            _logger = logger;
            _databaseContext = databaseContext;
            _cache = new(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromMinutes(2) });
        }

        public PollModel CreatePoll(string question, Dictionary<string, string?> options, DateTimeOffset expiresAt)
        {
            PollModel poll = new(Guid.NewGuid(), question, options, expiresAt);
            _databaseContext.Polls.Add(poll);
            _databaseContext.SaveChanges();
            _cache.Set(poll.Id, poll, CreateCancellationChangeToken(poll));
            return poll;
        }

        public PollModel? GetPollById(Guid pollId)
        {
            if (_cache.TryGetValue(pollId, out PollModel? poll))
            {
                _cache.Set(poll!.Id, poll, CreateCancellationChangeToken(poll)); // Reset the timeout everytime it's accessed.
                return poll;
            }

            poll = _databaseContext.Polls.FirstOrDefault(poll => poll.Id == pollId);
            if (poll != null)
            {
                _cache.Set(poll.Id, poll, CreateCancellationChangeToken(poll));
            }

            return poll;
        }

        public bool TryRemovePoll(Guid pollId, [NotNullWhen(true)] out PollModel? poll)
        {
            poll = GetPollById(pollId);
            if (poll == null)
            {
                return false;
            }

            _cache.Remove(pollId);
            _databaseContext.Polls.Remove(poll);
            _databaseContext.SaveChanges();
            return true;
        }

        public void SetVote(Guid pollId, ulong userId, int option)
        {
            PollModel poll = GetPollById(pollId)!;
            poll.Votes[userId] = option;
            _databaseContext.Update(poll);
            _databaseContext.SaveChanges();
            _cache.Set(poll.Id, poll, CreateCancellationChangeToken(poll));
        }

        public bool RemoveVote(Guid pollId, ulong userId)
        {
            PollModel poll = GetPollById(pollId)!;
            if (!poll.Votes.ContainsKey(userId))
            {
                return false;
            }

            poll.Votes.Remove(userId);
            _databaseContext.Update(poll);
            _databaseContext.SaveChanges();
            _cache.Set(poll.Id, poll, CreateCancellationChangeToken(poll));
            return true;
        }

        public int GetTotalVotes(Guid pollId) => GetPollById(pollId)?.Votes.Count ?? 0;

        private CancellationChangeToken CreateCancellationChangeToken(PollModel poll)
        {
            CancellationChangeToken cct = new(new CancellationTokenSource(_pollSaveDuration).Token);
            cct.RegisterChangeCallback(pollObject =>
            {
                PollModel poll = (PollModel)pollObject!;
                _databaseContext.Update(poll);
                if (_databaseContext.ChangeTracker.HasChanges())
                {
                    _databaseContext.SaveChanges();
                }
                _cache.Remove(poll.Id);
                _logger.LogInformation("Poll {PollId} has expired from the cache.", poll.Id);
            }, poll);
            return cct;
        }
    }
}
