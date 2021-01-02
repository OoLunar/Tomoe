using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Tomoe.Utils;

namespace Tomoe.Commands.Listeners
{
	public class ReactionAdded
	{
		private static readonly Logger Logger = new("Listeners/ReactionAdded");
		public static readonly List<Queue> QueueList = new();
		public delegate Task ReactionHandler(DiscordEmoji emoji);

		public struct Queue
		{
			public ulong MessageId { get; set; }
			public DiscordUser User { get; set; }
			public DiscordEmoji[] Emojis { get; set; }
			public ReactionHandler Action { get; set; }
		}

		public static async Task Handler(DiscordClient _, MessageReactionAddEventArgs eventArgs)
		{
			Logger.Trace($"Reaction recieved: {eventArgs.Emoji}");
			foreach (Queue queue in QueueList)
			{
				if (queue.User == eventArgs.User && queue.Emojis.Contains(eventArgs.Emoji) && eventArgs.Message.Id == queue.MessageId)
				{
					Logger.Debug($"Executing action for {queue.MessageId}");
					await queue.Action(eventArgs.Emoji);
					QueueList.Remove(queue);
				}
			}
			return;
		}
	}
}
