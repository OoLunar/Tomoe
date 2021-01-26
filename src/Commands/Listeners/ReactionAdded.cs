using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.EventArgs;

using Tomoe.Types;
using Tomoe.Utils;

namespace Tomoe.Commands.Listeners
{
	public class ReactionAdded
	{
		private static readonly Logger _logger = new("Commands.Listeners.ReactionAdded");
		internal static readonly List<Queue> _queueList = new();

		public static async Task Handler(DiscordClient _client, MessageReactionAddEventArgs eventArgs)
		{
			_logger.Trace($"Reaction recieved: {eventArgs.Emoji}");
			foreach (Queue queue in _queueList)
			{
				if (eventArgs.User.Id == Program.Client.CurrentUser.Id) continue;
				else if (queue.User.Id != eventArgs.User.Id) await eventArgs.Message.DeleteReactionAsync(eventArgs.Emoji, eventArgs.User, "Not the correct user.");
				else // if it is the requested user
				{
					if (queue.Type == Queue.ReactionType.Confirmation)
					{
						if (eventArgs.Emoji == Queue.ThumbsUp || eventArgs.Emoji == Queue.ThumbsDown)
						{
							await queue.Action.Invoke(eventArgs);
							_ = _queueList.Remove(queue);
							queue.Dispose();
						}
						else await eventArgs.Message.DeleteReactionAsync(eventArgs.Emoji, eventArgs.User, "Not the correct emoji.");
					}
					else if (queue.Type == Queue.ReactionType.Custom)
					{
						if (!queue.Emojis.Contains(eventArgs.Emoji)) await eventArgs.Message.DeleteReactionAsync(eventArgs.Emoji, eventArgs.User, "Not the correct emoji.");
						else
						{
							await queue.Action.Invoke(eventArgs);
							_ = _queueList.Remove(queue);
							queue.Dispose();
						}
					}
				}
			}
		}
	}
}