using EdgeDB;

namespace OoLunar.Tomoe.Database.Models
{
    /// <summary>
    /// Votes for a poll. Unfortunately cannot be rigged.
    /// </summary>
    [EdgeDBType("PollVote")]
    public sealed class PollVoteModel : DatabaseTrackable<PollVoteModel>
    {
        public PollModel Poll { get; private init; } = null!;
        public ulong VoterId { get; private init; }
        public PollOptionModel Option { get; private set; } = null!;

        public PollVoteModel() { }
        internal PollVoteModel(PollModel poll, ulong userId, PollOptionModel pollOption)
        {
            Poll = poll;
            VoterId = userId;
            Option = pollOption;
        }
    }
}
