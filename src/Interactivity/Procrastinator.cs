using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using OoLunar.Tomoe.Events;
using OoLunar.Tomoe.Interactivity.Data;

namespace OoLunar.Tomoe.Interactivity
{
    public sealed class Procrastinator : IEventHandler<InteractionCreatedEventArgs>
    {
        public ProcrastinatorConfiguration Configuration { get; init; }
        public IReadOnlyDictionary<Ulid, IdleData> Data => _data;

        private readonly Dictionary<Ulid, IdleData> _data = [];

        public Procrastinator(ProcrastinatorConfiguration? configuration = null) => Configuration = configuration ?? new();

        public async ValueTask PromptAsync(CommandContext context, string question, Func<PromptData, ValueTask> callback)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));
            ArgumentNullException.ThrowIfNull(callback, nameof(callback));

            PromptData data = new()
            {
                Id = Ulid.NewUlid(),
                Timeout = Configuration.DefaultTimeout,
                Question = question,
                Callback = callback
            };

            _data.Add(data.Id, data);
            if (context is TextCommandContext textContext)
            {
                DiscordMessageBuilder builder = new DiscordMessageBuilder()
                    .WithAllowedMentions(Mentions.None)
                    .WithContent(question)
                    .AddComponents(Configuration.ComponentController.CreateTextPromptButton(question, data.Id));

                await textContext.RespondAsync(builder);
            }
            else if (context is SlashCommandContext slashContext)
            {
                await slashContext.RespondWithModalAsync(new DiscordInteractionResponseBuilder()
                    .WithTitle(question)
                    .WithCustomId(data.Id.ToString())
                    .AddComponents(Configuration.ComponentController.CreateModalPromptButton(question, data.Id))
                );
            }
            else
            {
                _data.Remove(data.Id);
                throw new InvalidOperationException($"Unsupported context type: {context.GetType().Name}");
            }
        }

        public async ValueTask ChooseAsync(CommandContext context, string question, IReadOnlyList<string> options, Func<ChooseData, ValueTask> callback)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            ArgumentNullException.ThrowIfNull(callback, nameof(callback));

            ChooseData data = new()
            {
                Id = Ulid.NewUlid(),
                Timeout = Configuration.DefaultTimeout,
                Question = question,
                Options = options,
                Callback = callback
            };

            _data.Add(data.Id, data);
            await context.RespondAsync(new DiscordMessageBuilder()
                .WithAllowedMentions(Mentions.None)
                .WithContent(question)
                .AddComponents(Configuration.ComponentController.CreateChooseDropdown(question, options, data.Id))
            );
        }

        public async ValueTask ChooseMultipleAsync(CommandContext context, string question, IReadOnlyList<string> options, Func<ChooseMultipleData, ValueTask> callback)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            ArgumentNullException.ThrowIfNull(callback, nameof(callback));

            ChooseMultipleData data = new()
            {
                Id = Ulid.NewUlid(),
                Timeout = Configuration.DefaultTimeout,
                Question = question,
                Options = options,
                Callback = callback
            };

            _data.Add(data.Id, data);
            await context.RespondAsync(new DiscordMessageBuilder()
                .WithAllowedMentions(Mentions.None)
                .WithContent(question)
                .AddComponents(Configuration.ComponentController.CreateChooseMultipleDropdown(question, options, data.Id))
            );
        }

        [DiscordEvent(DiscordIntents.None)]
        public async Task HandleEventAsync(DiscordClient sender, InteractionCreatedEventArgs eventArgs)
        {
            if (!Ulid.TryParse(eventArgs.Interaction.Data.CustomId, out Ulid id) || !_data.TryGetValue(id, out IdleData? data))
            {
                return;
            }

            _data.Remove(id);
            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
            await (data switch
            {
                _ when data.IsTimedOut => HandleTimeoutAsync(eventArgs, data),
                PromptData promptData => HandlePromptAsync(eventArgs, promptData),
                ChooseData chooseData => HandleChooseAsync(eventArgs, chooseData),
                ChooseMultipleData chooseMultipleData => HandleChooseMultipleAsync(eventArgs, chooseMultipleData),
                _ => HandleUnknownAsync(eventArgs, data)
            });

            return;
        }

        public async ValueTask HandlePromptAsync(InteractionCreatedEventArgs eventArgs, PromptData data)
        {
            // If this is a modal submit
            if (eventArgs.Interaction.Type == DiscordInteractionType.ModalSubmit)
            {
                data.Response = eventArgs.Interaction.Data.Values[0];
                await data.Callback(data);
                return;
            }
            // Test to see if this is a text command turning into a button press
            else if (eventArgs.Interaction.Message?.ComponentActionRows is not null && eventArgs.Interaction.Message.ComponentActionRows.Count != 0)
            {
                foreach (DiscordComponent component in eventArgs.Interaction.Message.ComponentActionRows.SelectMany(row => row.Components))
                {
                    if (component is not DiscordButtonComponent button
                        || button.CustomId != eventArgs.Interaction.Data.CustomId
                        || button != Configuration.ComponentController.CreateTextPromptButton(data.Question, data.Id))
                    {
                        continue;
                    }

                    // Respond with the modal
                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, new DiscordInteractionResponseBuilder()
                        .WithTitle(data.Question)
                        .WithCustomId(data.Id.ToString())
                        .AddComponents(Configuration.ComponentController.CreateModalPromptButton(data.Question, data.Id))
                    );

                    return;
                }
            }

            // If neither of the above is true but the id still matches, this must be undefined behavior caused by an older or newer version of the lib
            // Just say that we don't know how to respond
            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("I don't know how to respond to this interaction.")
                .AsEphemeral(true)
            );
        }

        public async ValueTask HandleChooseAsync(InteractionCreatedEventArgs eventArgs, ChooseData data)
        {
            if (eventArgs.Interaction.Type == DiscordInteractionType.Component)
            {
                data.Response = eventArgs.Interaction.Data.Values[0];
                await data.Callback(data);
            }
            else
            {
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("I don't know how to respond to this interaction.")
                    .AsEphemeral(true)
                );
            }
        }

        public async ValueTask HandleChooseMultipleAsync(InteractionCreatedEventArgs eventArgs, ChooseMultipleData data)
        {
            if (eventArgs.Interaction.Type == DiscordInteractionType.Component)
            {
                data.Responses = eventArgs.Interaction.Data.Values;
                await data.Callback(data);
            }
            else
            {
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("I don't know how to respond to this interaction.")
                    .AsEphemeral(true)
                );
            }
        }

        public async ValueTask HandleTimeoutAsync(InteractionCreatedEventArgs eventArgs, IdleData data) => await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            .WithContent("The interaction has timed out.")
            .AsEphemeral(true)
        );

        public async ValueTask HandleUnknownAsync(InteractionCreatedEventArgs eventArgs, IdleData data) => await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            .WithContent("I don't know how to respond to this interaction.")
            .AsEphemeral(true)
        );
    }
}
