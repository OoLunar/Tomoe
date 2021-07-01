namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.EventArgs;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Utilities.Types;

    public partial class Listeners
    {
        public static Dictionary<string, QueueButton> QueueButtons { get; private set; } = new();

        public static async Task ButtonClicked(DiscordClient discordClient, ComponentInteractionCreateEventArgs componentInteractionCreateEventArgs)
        {
            string id = componentInteractionCreateEventArgs.Id.Split('-')[0];
            if (QueueButtons.TryGetValue(id, out QueueButton queueButton) && queueButton.UserId == componentInteractionCreateEventArgs.User.Id)
            {
                queueButton.SelectedButton = queueButton.Components.First(discordComponent => discordComponent.CustomId == componentInteractionCreateEventArgs.Id);
                QueueButtons.Remove(id);
                await componentInteractionCreateEventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate, new());
                componentInteractionCreateEventArgs.Handled = true;
            }
        }
    }
}
