using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using OoLunar.Tomoe.Events;
using OoLunar.Tomoe.Interactivity.Data;

namespace OoLunar.Tomoe.Interactivity
{
    public sealed partial class Procrastinator : IEventHandler<InteractionCreatedEventArgs>
    {
        [DiscordEvent(DiscordIntents.None)]
        public async Task HandleEventAsync(DiscordClient sender, InteractionCreatedEventArgs eventArgs)
        {
            if (eventArgs.Interaction.Data.CustomId.Length < 25
                || !Ulid.TryParse(eventArgs.Interaction.Data.CustomId[..26], out Ulid id)
                || !_data.TryGetValue(id, out IdleData? data)
                || data.CancellationToken.IsCancellationRequested
                || !await Configuration.ComponentHandler.HandleAnyAsync(this, eventArgs.Interaction, data)
                || !_data.Remove(id)
            )
            {
                return;
            }

            await (data switch
            {
                PromptData promptData => Configuration.ComponentHandler.HandlePromptAsync(this, eventArgs.Interaction, promptData),
                ChooseData chooseData => Configuration.ComponentHandler.HandleChooseAsync(this, eventArgs.Interaction, chooseData),
                ChooseMultipleData chooseMultipleData => Configuration.ComponentHandler.HandleChooseMultipleAsync(this, eventArgs.Interaction, chooseMultipleData),
                ConfirmData confirmData => Configuration.ComponentHandler.HandleConfirmAsync(this, eventArgs.Interaction, confirmData),
                _ => Configuration.ComponentHandler.HandleUnknownAsync(this, eventArgs.Interaction, data)
            });

            return;
        }
    }
}
