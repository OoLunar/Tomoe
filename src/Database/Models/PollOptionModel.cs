namespace OoLunar.Tomoe.Database.Models
{
    /// <summary>
    /// An option for a poll. These are what the user's vote for.
    /// </summary>
    public sealed class PollOptionModel : DatabaseTrackable<PollOptionModel>
    {
        /// <summary>
        /// The option's text.
        /// </summary>
        public string Option { get; private set; } = null!;

        /// <summary>
        /// The poll the option is for.
        /// </summary>
        public PollModel Poll { get; private set; } = null!;

        public PollOptionModel() { }
        public PollOptionModel(string option, PollModel poll)
        {
            Option = option;
            Poll = poll;
        }
    }
}
