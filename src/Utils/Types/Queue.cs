using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Tomoe.Commands.Listeners;

namespace Tomoe.Utils.Types
{
	public class Queue : IDisposable
	{
		public delegate Task ReactionHandler(QueueEventArgs eventArgs);
		public enum ReactionType
		{
			Custom,
			Confirmation
		}

		public DiscordMessage Message { get; private set; }
		public DiscordUser User { get; private set; }
		public DiscordEmoji[] Emojis { get; private set; }
		public ReactionHandler Action { get; private set; }
		public ReactionType Type { get; private set; }
		public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

		public Queue(DiscordMessage message, DiscordUser user, ReactionHandler action)
		{
			Message = message;
			User = user;
			Type = ReactionType.Confirmation;
			Action = action;
			message.DeleteAllReactionsAsync("Adding confirmation emojis...").GetAwaiter().GetResult();
			message.CreateReactionAsync(Constants.ThumbsUp).GetAwaiter().GetResult();
			message.CreateReactionAsync(Constants.ThumbsDown).GetAwaiter().GetResult();
			ReactionAdded.QueueList.Add(this);
		}

		public Queue(DiscordMessage message, DiscordUser user, DiscordEmoji[] emojis, ReactionHandler action)
		{
			Message = message;
			User = user;
			Emojis = emojis;
			Action = action;
			Type = ReactionType.Custom;
			message.DeleteAllReactionsAsync().GetAwaiter().GetResult();
			for (int i = 0; i < emojis.Length; i++) message.CreateReactionAsync(emojis[i]).GetAwaiter().GetResult();
			ReactionAdded.QueueList.Add(this);
		}

		public async Task WaitForReaction()
		{
			while (ReactionAdded.QueueList.Contains(this)) await Task.Delay(100);
		}

		public void Dispose()
		{
			_ = ReactionAdded.QueueList.Remove(this);
			Message = null;
			User = null;
			Emojis = null;
			Action = null;
			GC.SuppressFinalize(this);
		}
	}
}
