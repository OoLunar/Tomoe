using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace Tomoe.Utils {
    class ReactionCallBack : IReactionCallback {
        // TODO: Find other ways to use reactions. Potentionally, make our own paged embeds.
        public enum ActionTypes {
            Boolean,
            PagedEmbed
        }

        public delegate void Reaction(object source, DialogContext context, SocketReaction reaction);

        public SocketCommandContext Context { get; set; }
        public RunMode RunMode => RunMode.Async;
        public ICriterion<SocketReaction> Criterion => new EmptyCriterion<SocketReaction>();
        public TimeSpan? Timeout => System.TimeSpan.FromSeconds(10);
        public DialogContext DialogContext;
        public ActionTypes TakeAction;
        public event Reaction OnReaction;

        // Returns true when we're done listening, false when we're not... 
        public async Task<bool> HandleCallbackAsync(SocketReaction reaction) {
            if (TakeAction == ActionTypes.Boolean) {
                // Check if it's the same person reacting. Returning false means keep listening for reactions.
                if (reaction.UserId != DialogContext.Issuer.Id || reaction.Emote.Name != Program.Dialogs.Emotes.No && reaction.Emote.Name != Program.Dialogs.Emotes.Yes) {
                    // Remove outsiders reaction regardless of emote, as they aren't the one who initiated the command.
                    await reaction.Channel.GetCachedMessage(reaction.MessageId).RemoveReactionAsync(reaction.Emote, reaction.UserId);
                    return false;
                }
                // Exit
                else if (reaction.Emote.Name == Program.Dialogs.Emotes.No) {
                    DialogContext.Error = Program.Dialogs.Message.Setup.Errors.Exit;
                    DialogContext.Channel = reaction.Channel;
                    await DialogContext.SendChannel();
                    return true;
                }
                // Success
                else if (reaction.Emote.Name == Program.Dialogs.Emotes.Yes) {
                    DialogContext.Channel = reaction.Channel;
                    DialogContext.OldMessage = reaction.Message.Value ?? null;
                    OnReaction.Invoke(this, DialogContext, reaction);
                    return true;
                }
            }
            return false;
        }
    }
}