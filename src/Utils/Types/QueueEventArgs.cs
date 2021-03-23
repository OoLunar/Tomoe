using DSharpPlus.EventArgs;

namespace Tomoe.Utils.Types
{
	public class QueueEventArgs
	{
		public MessageReactionAddEventArgs MessageReactionAddEventArgs { get; internal set; }
		public bool TimedOut { get; internal set; }
	}
}
