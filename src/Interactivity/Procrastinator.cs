using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.Data;

namespace OoLunar.Tomoe.Interactivity
{
    public sealed partial class Procrastinator : IDisposable
    {
        public ProcrastinatorConfiguration Configuration { get; init; }
        public IReadOnlyDictionary<Ulid, IdleData> Data => _data;

        private readonly Dictionary<Ulid, IdleData> _data = [];
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public Procrastinator(ProcrastinatorConfiguration? configuration = null)
        {
            Configuration = configuration ?? new();

            PeriodicTimer timer = new(Configuration.TimeoutCheckInterval);
            _cancellationTokenSource.Token.Register(timer.Dispose);
            _ = TimeoutTimerAsync(timer);
        }

        public bool TryAddData(Ulid id, IdleData data) => _data.TryAdd(id, data);

        public async ValueTask<string?> PromptAsync(CommandContext context, string question)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));

            PromptData data = new()
            {
                Id = Ulid.NewUlid(),
                Timeout = Configuration.DefaultTimeout,
                Question = question
            };

            _data.Add(data.Id, data);
            if (context is TextCommandContext textContext)
            {
                DiscordButtonComponent button = Configuration.ComponentController.CreateTextPromptButton(question, data.Id);
                if (!button.CustomId.StartsWith(data.Id.ToString(), StringComparison.Ordinal))
                {
                    _data.Remove(data.Id);
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
                    .WithCustomId(data.Id.ToString())
                    .AddComponents(Configuration.ComponentController.CreateModalPromptButton(question, data.Id))
                );
            }
            else
            {
                _data.Remove(data.Id);
                throw new InvalidOperationException($"Unsupported context type: {context.GetType().Name}");
            }

            await data.TaskCompletionSource.Task;
            return data.TaskCompletionSource.Task.Result;
        }

        public async ValueTask<string?> ChooseAsync(CommandContext context, string question, IReadOnlyList<string> options)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            ChooseData data = new()
            {
                Id = Ulid.NewUlid(),
                Timeout = Configuration.DefaultTimeout,
                Question = question,
                Options = options
            };

            DiscordSelectComponent dropDown = Configuration.ComponentController.CreateChooseDropdown(question, options, data.Id);
            if (!dropDown.CustomId.StartsWith(data.Id.ToString(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The custom id of the select menu must start with the id of the data.");
            }

            _data.Add(data.Id, data);
            await context.RespondAsync(new DiscordMessageBuilder()
                .WithAllowedMentions(Mentions.None)
                .WithContent(question)
                .AddComponents(dropDown)
            );

            data.Message = await context.GetResponseAsync();
            await data.TaskCompletionSource.Task;
            return data.TaskCompletionSource.Task.Result;
        }

        public async ValueTask<IReadOnlyList<string>> ChooseMultipleAsync(CommandContext context, string question, IReadOnlyList<string> options)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            ChooseMultipleData data = new()
            {
                Id = Ulid.NewUlid(),
                Timeout = Configuration.DefaultTimeout,
                Question = question,
                Options = options
            };

            DiscordSelectComponent dropDown = Configuration.ComponentController.CreateChooseMultipleDropdown(question, options, data.Id);
            if (!dropDown.CustomId.StartsWith(data.Id.ToString(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The custom id of the select menu must start with the id of the data.");
            }

            _data.Add(data.Id, data);
            await context.RespondAsync(new DiscordMessageBuilder()
                .WithAllowedMentions(Mentions.None)
                .WithContent(question)
                .AddComponents(dropDown)
            );

            data.Message = await context.GetResponseAsync();
            await data.TaskCompletionSource.Task;
            return data.TaskCompletionSource.Task.Result;
        }

        public async ValueTask<bool?> ConfirmAsync(CommandContext context, string question)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));

            ConfirmData data = new()
            {
                Id = Ulid.NewUlid(),
                Timeout = Configuration.DefaultTimeout,
                Question = question
            };

            List<DiscordButtonComponent> buttons = [Configuration.ComponentController.CreateConfirmButton(question, data.Id, true), Configuration.ComponentController.CreateConfirmButton(question, data.Id, false)];
            for (int i = 0; i < buttons.Count; i++)
            {
                DiscordButtonComponent button = buttons[i];
                if (!button.CustomId.StartsWith(data.Id.ToString(), StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"The custom id of the button must start with the id of the data. Index: {i}");
                }
            }

            _data.Add(data.Id, data);
            await context.RespondAsync(new DiscordMessageBuilder()
                .WithAllowedMentions(Mentions.None)
                .WithContent(question)
                .AddComponents(buttons)
            );

            data.Message = await context.GetResponseAsync();
            await data.TaskCompletionSource.Task;
            return data.TaskCompletionSource.Task.Result;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _data.Clear();
        }
    }
}
