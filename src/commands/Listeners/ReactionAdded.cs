using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Tomoe.Utils;

namespace Tomoe.Commands.Listeners {
    public class ReactionAdded {
        private static Logger _logger = new Logger("Listeners/ReactionAdded");
        public static List<Queue> QueueList = new List<Queue>();
        public delegate Task ReactionHandler(DiscordEmoji emoji);

        public struct Queue {
            public ulong MessageId { get; set; }
            public DiscordUser User { get; set; }
            public DiscordEmoji[] Emojis { get; set; }
            public ReactionHandler Action { get; set; }
        }

        public static async Task Handler(DiscordClient client, DSharpPlus.EventArgs.MessageReactionAddEventArgs eventArgs) {
            _logger.Trace($"Reaction recieved: {eventArgs.Emoji}");
            foreach (Queue queue in QueueList) {
                if (queue.User == eventArgs.User && queue.Emojis.Contains(eventArgs.Emoji) && eventArgs.Message.Id == queue.MessageId) {
                    _logger.Debug($"Executing action for {queue.MessageId}");
                    await queue.Action(eventArgs.Emoji);
                    QueueList.Remove(queue);
                }
            }
            return;
        }
    }
}