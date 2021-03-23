using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Tomoe.Utils.Types;

namespace Tomoe.Commands.Listeners
{
	public class ReactionAdded
	{
		internal static readonly List<Queue> QueueList = new();

		/// <summary>
		/// Custom reaction handler. InteractivityExtension.WaitForReactionAsync() is difficult to use with a lot of code. This allows for easy "yes" or "no" reaction prompts, or reaction prompts with custom emoji's that only require creating a new <see cref="Queue"/>.
		/// </summary>
		/// <param name="_client">Unused <see cref="DiscordClient"/>.</param>
		/// <param name="eventArgs">Used to get the reaction and who's reacting.</param>
		public static async Task Handler(DiscordClient _client, MessageReactionAddEventArgs eventArgs)
		{
			// Call ToList to create a clone, which prevents https://stackoverflow.com/questions/33703494/foreach-collection-was-modified-enumeration-operation-may-not-execute
			foreach (Queue queue in QueueList.ToList())
			{
				QueueEventArgs queueEventArgs = new();
				queueEventArgs.MessageReactionAddEventArgs = eventArgs;
				if (eventArgs.User.Id == Program.Client.CurrentUser.Id) continue;
				/*
				else if ((queue.CreatedAt + Program.Config.ReactionTimeout) > DateTime.UtcNow)
				{
					// When invoking, the delegate should be checking if the queue had timed out or not.
					queueEventArgs.TimedOut = true;
					await queue.Action.Invoke(queueEventArgs);
					_ = QueueList.Remove(queue);
					queue.Dispose();
				}
				*/
				else if (queue.User.Id != eventArgs.User.Id) await eventArgs.Message.DeleteReactionAsync(eventArgs.Emoji, eventArgs.User, "Not the correct user.");
				else
				{
					// Check if the Queue is a "yes or no" prompt, or a custom prompt
					if (queue.Type == Queue.ReactionType.Confirmation)
					{
						if (eventArgs.Emoji == Constants.ThumbsUp || eventArgs.Emoji == Constants.ThumbsDown)
						{
							await queue.Action.Invoke(queueEventArgs);
							_ = QueueList.Remove(queue);
							queue.Dispose();
						}
						else await eventArgs.Message.DeleteReactionAsync(eventArgs.Emoji, eventArgs.User, "Not the correct emoji.");
					}
					else if (queue.Type == Queue.ReactionType.Custom)
					{
						if (!queue.Emojis.Contains(eventArgs.Emoji)) await eventArgs.Message.DeleteReactionAsync(eventArgs.Emoji, eventArgs.User, "Not the correct emoji.");
						else
						{
							await queue.Action.Invoke(queueEventArgs);
							_ = QueueList.Remove(queue);
							queue.Dispose();
						}
					}
				}
			}
		}
	}
}
