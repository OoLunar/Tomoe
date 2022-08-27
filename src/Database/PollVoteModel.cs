using System;
using System.Collections.Generic;
using EdgeDB;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Database
{
    /// <summary>
    /// Votes for a poll. Unfortunately cannot be rigged.
    /// </summary>
    [EdgeDBType("PollVote")]
    public sealed class PollVoteModel : ICopyable<PollVoteModel>
    {
        public Guid? Id { get; private set; }
        public PollModel Poll { get; private init; } = null!;
        public ulong VoterId { get; private init; }
        public PollOptionModel Option { get; private set; }

        [EdgeDBDeserializer]
        private PollVoteModel(IDictionary<string, object?> raw)
        {
            Id = (Guid?)raw["id"];
            Poll = (PollModel)raw["poll"]!;
            VoterId = (ulong)raw["user_id"]!;
            Option = (PollOptionModel)raw["option"]!;
        }

        internal PollVoteModel(Guid? id, PollModel poll, ulong userId, PollOptionModel pollOption)
        {
            Id = id;
            Poll = poll;
            VoterId = userId;
            Option = pollOption;
        }

        public PollVoteModel Copy(PollVoteModel old)
        {
            Id = old.Id;
            Option = old.Option;
            return this;
        }
    }
}
