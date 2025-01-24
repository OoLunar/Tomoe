using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.Moments.Idle;

namespace OoLunar.Tomoe.Interactivity.Moments.Prompt
{
    public record PromptMoment : IdleMoment<IPromptComponentCreator>
    {
        public required string Question { get; init; }
        public TaskCompletionSource<string?> TaskCompletionSource { get; init; } = new();

        public override async ValueTask HandleAsync(Procrastinator procrastinator, DiscordInteraction interaction)
        {
            // If this is a modal submit
            if (interaction.Type == DiscordInteractionType.ModalSubmit)
            {
                DiscordTextInputComponent? textInputComponent = interaction.Data.TextInputComponents?.FirstOrDefault(component => component.CustomId == Id.ToString());
                if (textInputComponent is null)
                {
                    return;
                }

                TaskCompletionSource.SetResult(textInputComponent.Value);
                await interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            }

            // Try to find the text button, if applicable, and disable it
            DiscordButtonComponent findButton = ComponentCreator.CreateTextPromptButton(Question, Id);
            if (interaction.Message?.Components?.Count is not null and not 0
                && interaction.Message.FilterComponents<DiscordButtonComponent>().Any(button => button.CustomId == findButton.CustomId))
            {
                if (interaction.Type == DiscordInteractionType.Component)
                {
                    // Readd the data
                    if (!procrastinator.TryAddData(Id, this))
                    {
                        // This shouldn't ever happen, but just in case
                        throw new InvalidOperationException("The data could not be added to the dictionary.");
                    }

                    // Send the modal
                    await interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, new DiscordInteractionResponseBuilder()
                        .WithTitle(Question)
                        .WithCustomId(Id.ToString())
                        .AddComponents(ComponentCreator.CreateModalPromptButton(Question, Id))
                    );
                }
                else if (interaction.Type == DiscordInteractionType.ModalSubmit)
                {
                    // Update the text button to a disabled state
                    DiscordWebhookBuilder responseBuilder = new(new DiscordMessageBuilder(interaction.Message));
                    responseBuilder.ClearComponents();
                    responseBuilder.AddComponents(interaction.Message.Components.Mutate<DiscordButtonComponent>(
                        button => button.CustomId == findButton.CustomId,
                        button => button.Disable()).Cast<DiscordActionRowComponent>());

                    await interaction.EditOriginalResponseAsync(responseBuilder);
                }
            }
        }
    }
}
