using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using Tomoe.Commands.Listeners;
using System;

namespace Tomoe.Types
{
	public class Queue : IDisposable
	{
		public delegate Task ReactionHandler(MessageReactionAddEventArgs eventArgs);
		public enum ReactionType
		{
			Custom,
			Confirmation
		}

		public static readonly DiscordEmoji ThumbsUp = DiscordEmoji.FromUnicode(Program.Client.ShardClients[0], "👍");
		public static readonly DiscordEmoji ThumbsDown = DiscordEmoji.FromUnicode(Program.Client.ShardClients[0], "👎");

		public DiscordMessage Message { get; private set; }
		public DiscordUser User { get; private set; }
		public DiscordEmoji[] Emojis { get; private set; }
		public ReactionHandler Action { get; private set; }
		public ReactionType Type { get; private set; }

		public Queue(DiscordMessage message, DiscordUser user, ReactionHandler action)
		{
			Message = message;
			User = user;
			Type = ReactionType.Confirmation;
			Action = action;
			message.DeleteAllReactionsAsync("Adding confirmation emoji's.").GetAwaiter().GetResult();
			message.CreateReactionAsync(ThumbsUp).GetAwaiter().GetResult();
			message.CreateReactionAsync(ThumbsDown).GetAwaiter().GetResult();
			ReactionAdded._queueList.Add(this);
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
			ReactionAdded._queueList.Add(this);
		}

		public async Task WaitForReaction()
		{
			while (ReactionAdded._queueList.Contains(this)) Task.Delay(500);
		}

		public void Dispose()
		{
			Message = null;
			User = null;
			Emojis = null;
			Action = null;
			GC.SuppressFinalize(this);
		}
	}
}
