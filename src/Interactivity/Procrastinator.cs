using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.ComponentCreators;
using OoLunar.Tomoe.Interactivity.Data;

namespace OoLunar.Tomoe.Interactivity
{
    public sealed partial class Procrastinator
    {
        public ProcrastinatorConfiguration Configuration { get; init; }
        public IReadOnlyDictionary<Ulid, IdleData> Data => _data;

        private readonly Dictionary<Ulid, IdleData> _data = [];

        public Procrastinator(ProcrastinatorConfiguration? configuration = null) => Configuration = configuration ?? new();

        public bool TryAddData(Ulid id, IdleData data) => _data.TryAdd(id, data);

        public async ValueTask<string?> PromptAsync(CommandContext context, string question, IComponentCreator? componentCreator = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));
            componentCreator ??= Configuration.ComponentController;

            Ulid id = Ulid.NewUlid();
            PromptData data = new()
            {
                Id = id,
                AuthorId = context.User.Id,
                CancellationToken = CreateCancellationToken(id, cancellationToken),
                Question = question
            };

            _data.Add(id, data);
            if (context is TextCommandContext textContext)
            {
                DiscordButtonComponent button = componentCreator.CreateTextPromptButton(question, id);
                if (!button.CustomId.StartsWith(id.ToString(), StringComparison.Ordinal))
                {
                    _data.Remove(id);
                    throw new InvalidOperationException("The custom id of the button must start with the id of the data.");
                }

                DiscordMessageBuilder builder = new DiscordMessageBuilder()
                    .WithAllowedMentions(Mentions.None)
                    .WithContent(question)
                    .AddComponents(button);

                await textContext.RespondAsync(builder);
                data.Message = textContext.Response;
            }
            else if (context is SlashCommandContext slashContext)
            {
                await slashContext.RespondWithModalAsync(new DiscordInteractionResponseBuilder()
                    .WithTitle(question)
                    .WithCustomId(id.ToString())
                    .AddComponents(componentCreator.CreateModalPromptButton(question, id))
                );
            }
            else
            {
                _data.Remove(id);
                throw new InvalidOperationException($"Unsupported context type: {context.GetType().Name}");
            }

            await data.TaskCompletionSource.Task;
            return data.TaskCompletionSource.Task.Result;
        }

        public async ValueTask<string?> ChooseAsync(CommandContext context, string question, IReadOnlyList<string> options, IComponentCreator? componentCreator = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            componentCreator ??= Configuration.ComponentController;

            Ulid id = Ulid.NewUlid();
            ChooseData data = new()
            {
                Id = id,
                AuthorId = context.User.Id,
                CancellationToken = CreateCancellationToken(id, cancellationToken),
                Question = question,
                Options = options
            };

            DiscordSelectComponent dropDown = componentCreator.CreateChooseDropdown(question, options, id);
            if (!dropDown.CustomId.StartsWith(id.ToString(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The custom id of the select menu must start with the id of the data.");
            }

            _data.Add(id, data);
            await context.RespondAsync(new DiscordMessageBuilder()
                .WithAllowedMentions(Mentions.None)
                .WithContent(question)
                .AddComponents(dropDown)
            );

            data.Message = await context.GetResponseAsync();
            await data.TaskCompletionSource.Task;
            return data.TaskCompletionSource.Task.Result;
        }

        public async ValueTask<IReadOnlyList<string>> ChooseMultipleAsync(CommandContext context, string question, IReadOnlyList<string> options, IComponentCreator? componentCreator = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            componentCreator ??= Configuration.ComponentController;

            Ulid id = Ulid.NewUlid();
            ChooseMultipleData data = new()
            {
                Id = id,
                AuthorId = context.User.Id,
                CancellationToken = CreateCancellationToken(id, cancellationToken),
                Question = question,
                Options = options
            };

            DiscordSelectComponent dropDown = componentCreator.CreateChooseMultipleDropdown(question, options, id);
            if (!dropDown.CustomId.StartsWith(id.ToString(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The custom id of the select menu must start with the id of the data.");
            }

            _data.Add(id, data);
            await context.RespondAsync(new DiscordMessageBuilder()
                .WithAllowedMentions(Mentions.None)
                .WithContent(question)
                .AddComponents(dropDown)
            );

            data.Message = await context.GetResponseAsync();
            await data.TaskCompletionSource.Task;
            return data.TaskCompletionSource.Task.Result;
        }

        public async ValueTask<bool?> ConfirmAsync(CommandContext context, string question, IComponentCreator? componentCreator = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));
            componentCreator ??= Configuration.ComponentController;

            Ulid id = Ulid.NewUlid();
            ConfirmData data = new()
            {
                Id = id,
                AuthorId = context.User.Id,
                CancellationToken = CreateCancellationToken(id, cancellationToken),
                Question = question
            };

            List<DiscordButtonComponent> buttons = [componentCreator.CreateConfirmButton(question, id, true), componentCreator.CreateConfirmButton(question, id, false)];
            for (int i = 0; i < buttons.Count; i++)
            {
                DiscordButtonComponent button = buttons[i];
                if (!button.CustomId.StartsWith(id.ToString(), StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"The custom id of the button must start with the id of the data. Index: {i}");
                }
            }

            _data.Add(id, data);
            await context.RespondAsync(new DiscordMessageBuilder()
                .WithAllowedMentions(Mentions.None)
                .WithContent(question)
                .AddComponents(buttons)
            );

            data.Message = await context.GetResponseAsync();
            await data.TaskCompletionSource.Task;
            return data.TaskCompletionSource.Task.Result;
        }

        private CancellationToken CreateCancellationToken(Ulid id, CancellationToken cancellationToken = default)
        {
            if (cancellationToken == default)
            {
                cancellationToken = new CancellationTokenSource(Configuration.DefaultTimeout).Token;
            }

            cancellationToken.Register(static async (object? obj) =>
            {
                if (obj is (Procrastinator procrastinator, Ulid id) && procrastinator._data.Remove(id, out IdleData? data))
                {
                    await procrastinator.Configuration.ComponentHandler.HandleTimedOutAsync(data);
                }
            }, (this, id));

            return cancellationToken;
        }
    }
}
