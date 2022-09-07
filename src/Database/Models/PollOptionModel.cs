namespace OoLunar.Tomoe.Database.Models
{
    public sealed class PollOptionModel : DatabaseTrackable<PollOptionModel>
    {
        public string Option { get; private init; } = null!;
        public PollModel Poll { get; private init; } = null!;

        public PollOptionModel() { }
        public PollOptionModel(string option, PollModel poll)
        {
            Option = option;
            Poll = poll;
        }
    }
}
