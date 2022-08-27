using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Database
{
    public sealed partial class PollModel : ICopyable<PollModel>
    {
        private const string AddVoteQuery = @"
            INSERT PollVote {
                poll := $poll,
                user_id := <str>$userId,
                option := (SELECT PollOption FILTER .option = <str>$optionId AND .poll = poll)
            };";

        private const string RemoveVoteQuery = @"
            DELETE PollVote
            FILTER .user_id = <str>$userId AND .poll = $poll;";

        private const string DeleteQuery = @"
            DELETE Poll
            FILTER .id = $pollId;";
    }
}
