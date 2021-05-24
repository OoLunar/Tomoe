namespace Tomoe.Utils.Types
{
    using DSharpPlus.EventArgs;

    public class QueueEventArgs
    {
        public MessageReactionAddEventArgs MessageReactionAddEventArgs { get; internal set; }
        public bool TimedOut { get; internal set; }
    }
}
