using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.Data;

namespace OoLunar.Tomoe.Interactivity.ComponentHandlers
{
    public sealed class DefaultComponentHandler : IComponentHandler
    {
        public bool AllowAnyone { get; init; }

        public DefaultComponentHandler(bool allowAnyone = false) => AllowAnyone = allowAnyone;

        public async Task<bool> HandleAnyAsync(Procrastinator procrastinator, DiscordInteraction interaction, IdleData data)
        {
            if (AllowAnyone || data.AuthorId == interaction.User.Id)
            {
                return true;
            }

            await interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("This message wasn't meant for you.")
                .AsEphemeral(true)
            );

            return false;
        }

        public async Task HandlePromptAsync(Procrastinator procrastinator, DiscordInteraction interaction, PromptData data)
        {
            // If this is a modal submit
            if (interaction.Type == DiscordInteractionType.ModalSubmit)
            {
                DiscordTextInputComponent? textInputComponent = interaction.Data.TextInputComponents?.FirstOrDefault(component => component.CustomId == data.Id.ToString());
                if (textInputComponent is null)
                {
                    await HandleUnknownAsync(procrastinator, interaction, data);
                    return;
                }

                data.TaskCompletionSource.SetResult(textInputComponent.Value);
                await interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            }

            // Try to find the text button, if applicable, and disable it
            DiscordButtonComponent findButton = procrastinator.Configuration.ComponentController.CreateTextPromptButton(data.Question, data.Id);
            if (interaction.Message?.Components is not null
                && interaction.Message.Components.Count != 0
                && interaction.Message.FilterComponents<DiscordButtonComponent>().Count(button => button == findButton) == 1)
            {
                if (interaction.Type == DiscordInteractionType.Component)
                {
                    // Readd the data
                    if (!procrastinator.TryAddData(data.Id, data))
                    {
                        // This shouldn't ever happen, but just in case
                        await HandleUnknownAsync(procrastinator, interaction, data);
                        return;
                    }

                    // Send the modal
                    await interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, new DiscordInteractionResponseBuilder()
                        .WithTitle(data.Question)
                        .WithCustomId(data.Id.ToString())
                        .AddComponents(procrastinator.Configuration.ComponentController.CreateModalPromptButton(data.Question, data.Id))
                    );
                }
                else if (interaction.Type == DiscordInteractionType.ModalSubmit)
                {
                    // Update the text button to a disabled state
                    DiscordWebhookBuilder responseBuilder = new(new DiscordMessageBuilder(interaction.Message));
                    responseBuilder.ClearComponents();
                    responseBuilder.AddComponents(interaction.Message.Components.Mutate<DiscordButtonComponent>(
                        button => button == findButton,
                        button => button.Disable()).Cast<DiscordActionRowComponent>());

                    await interaction.EditOriginalResponseAsync(responseBuilder);
                }
            }

            if (interaction.ResponseState == DiscordInteractionResponseState.Unacknowledged)
            {
                await HandleUnknownAsync(procrastinator, interaction, data);
            }
        }

        public async Task HandlePickAsync(Procrastinator procrastinator, DiscordInteraction interaction, PickData data)
        {
            if (interaction.Type != DiscordInteractionType.Component
                || interaction.Message?.Components is null
                || interaction.Message.Components.Count == 0)
            {
                await HandleUnknownAsync(procrastinator, interaction, data);
                return;
            }

            data.TaskCompletionSource.SetResult(interaction.Data.Values[0]);

            DiscordInteractionResponseBuilder responseBuilder = new(new DiscordMessageBuilder(interaction.Message));
            responseBuilder.ClearComponents();
            responseBuilder.AddComponents(interaction.Message.Components.Mutate<DiscordSelectComponent>(
                select => select.CustomId.StartsWith(data.Id.ToString(), StringComparison.Ordinal),
                select =>
                {
                    List<DiscordSelectComponentOption> options = [];
                    foreach (DiscordSelectComponentOption option in select.Options)
                    {
                        options.Add(new DiscordSelectComponentOption(
                            option.Label,
                            option.Value,
                            option.Description,
                            option.Value == interaction.Data.Values[0],
                            option.Emoji
                        ));
                    }

                    return new DiscordSelectComponent(select.CustomId, select.Placeholder, options, true, select.MinimumSelectedValues ?? 1, select.MaximumSelectedValues ?? 1);
                }
            ).Cast<DiscordActionRowComponent>());

            await interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
        }

        public async Task HandleChooseAsync(Procrastinator procrastinator, DiscordInteraction interaction, ChooseData data)
        {
            if (interaction.Type != DiscordInteractionType.Component
                || interaction.Message?.Components is null
                || interaction.Message.Components.Count == 0)
            {
                await HandleUnknownAsync(procrastinator, interaction, data);
                return;
            }

            data.TaskCompletionSource.SetResult(interaction.Data.Values);

            DiscordInteractionResponseBuilder responseBuilder = new(new DiscordMessageBuilder(interaction.Message));
            responseBuilder.ClearComponents();
            responseBuilder.AddComponents(interaction.Message.Components.Mutate<DiscordSelectComponent>(
                select => select.CustomId.StartsWith(data.Id.ToString(), StringComparison.Ordinal),
                select =>
                {
                    List<DiscordSelectComponentOption> options = [];
                    foreach (DiscordSelectComponentOption option in select.Options)
                    {
                        options.Add(new DiscordSelectComponentOption(
                            option.Label,
                            option.Value,
                            option.Description,
                            option.Value == interaction.Data.Values[0],
                            option.Emoji
                        ));
                    }

                    return new DiscordSelectComponent(select.CustomId, select.Placeholder, options, true, select.MinimumSelectedValues ?? 1, select.MaximumSelectedValues ?? 1);
                }
            ).Cast<DiscordActionRowComponent>());

            await interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
        }

        public async Task HandleConfirmAsync(Procrastinator procrastinator, DiscordInteraction interaction, ConfirmData data)
        {
            if (interaction.Type != DiscordInteractionType.Component
                || interaction.Message?.Components is null
                || interaction.Message.Components.Count == 0)
            {
                await HandleUnknownAsync(procrastinator, interaction, data);
                return;
            }

            DiscordInteractionResponseBuilder responseBuilder = new(new DiscordMessageBuilder(interaction.Message));
            responseBuilder.ClearComponents();
            responseBuilder.AddComponents(interaction.Message.Components.Mutate<DiscordButtonComponent>(
                button => button.CustomId.StartsWith(data.Id.ToString(), StringComparison.Ordinal) && !button.Disabled,
                button =>
                {
                    if (button.CustomId == interaction.Data.CustomId)
                    {
                        data.TaskCompletionSource.SetResult(button == procrastinator.Configuration.ComponentController.CreateConfirmButton(data.Question, data.Id, true));
                    }

                    return button.Disable();
                }
            ).Cast<DiscordActionRowComponent>());

            await interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
        }

        public async Task HandleUnknownAsync(Procrastinator procrastinator, DiscordInteraction interaction, IdleData data) => await interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            .WithContent("I don't know how to respond to this interaction.")
            .AsEphemeral(true)
        );

        public async Task HandleTimedOutAsync(IdleData data)
        {
            if (data is PromptData promptData)
            {
                promptData.TaskCompletionSource.TrySetResult(null);
            }
            else if (data is PickData pickData)
            {
                pickData.TaskCompletionSource.TrySetResult(null);
            }
            else if (data is ChooseData chooseData)
            {
                chooseData.TaskCompletionSource.TrySetResult([]);
            }
            else if (data is ConfirmData confirmData)
            {
                confirmData.TaskCompletionSource.TrySetResult(null);
            }

            // If there's a message attached, disable all components related to the data
            if (data.Message is not null
                && data.Message.Components is not null
                && data.Message.Components.Count != 0)
            {
                DiscordMessageBuilder messageBuilder = new(data.Message);
                messageBuilder.ClearComponents();
                foreach (DiscordComponent component in DisableComponents(data.Id.ToString(), data.Message.Components))
                {
                    // We do this because ARC has it's own overload
                    if (component is DiscordActionRowComponent row)
                    {
                        messageBuilder.AddComponents(Enumerable.Repeat(row, 1));
                    }
                    else
                    {
                        messageBuilder.AddComponents(component);
                    }
                }

                // Disable the component
                await data.Message.ModifyAsync(messageBuilder);
            }
        }

        private static IEnumerable<DiscordComponent> DisableComponents(string id, IEnumerable<DiscordComponent> components)
        {
            foreach (DiscordComponent component in components)
            {
                if (component is DiscordActionRowComponent row)
                {
                    yield return new DiscordActionRowComponent(DisableComponents(id, row.Components));
                }
                // If the component is not related to the data, ignore it.
                else if (!component.CustomId.StartsWith(id, StringComparison.Ordinal))
                {
                    yield return component;
                }
                else if (component is DiscordButtonComponent button)
                {
                    yield return button.Disable();
                }
                else if (component is DiscordSelectComponent select)
                {
                    yield return new DiscordSelectComponent(select.CustomId, select.Placeholder, select.Options, select.Disabled, select.MinimumSelectedValues ?? 1, select.MaximumSelectedValues ?? 1);
                }
            }
        }
    }
}
