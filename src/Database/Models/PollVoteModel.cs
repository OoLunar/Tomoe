using EdgeDB;

namespace OoLunar.Tomoe.Database.Models
{
    /// <summary>
    /// Votes for a poll. Unfortunately cannot be rigged.
    /// </summary>
    [EdgeDBType("PollVote")]
    public sealed class PollVoteModel : DatabaseTrackable<PollVoteModel>
    {
        /// <summary>
        /// The poll that the vote is attached too.
        /// </summary>
        public PollModel Poll { get; private init; } = null!;

        /// <summary>
        /// Who the vote represents.
        /// </summary>
        public ulong VoterId { get; private init; }

        /// <summary>
        /// Which option the voter is voting for.
        /// </summary>
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
