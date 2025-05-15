using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.Tomoe.Interactivity.Moments.Prompt
{
    public static class PromptExtensions
    {
        public static async ValueTask<string?> PromptAsync(this DiscordMember member, Procrastinator procrastinator, string question, string placeholder, IPromptComponentCreator? componentCreator = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(member, nameof(member));
            ArgumentNullException.ThrowIfNull(question, nameof(question));
            ArgumentNullException.ThrowIfNull(placeholder, nameof(placeholder));
            ArgumentNullException.ThrowIfNull(procrastinator, nameof(procrastinator));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(question.Length, 45, nameof(question));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(placeholder.Length, 100, nameof(placeholder));
            componentCreator ??= procrastinator.Configuration.GetComponentCreatorOrDefault<IPromptComponentCreator, PromptDefaultComponentCreator>();

            Ulid id = Ulid.NewUlid();
            PromptMoment data = new()
            {
                Id = id,
                AuthorId = member.Id,
                CancellationToken = procrastinator.RegisterTimeoutCallback(id, cancellationToken),
                ComponentCreator = componentCreator,
                Question = question,
                Placeholder = placeholder
            };

            DiscordButtonComponent button = componentCreator.CreateTextPromptButton(question, id);
            if (!button.CustomId.StartsWith(id.ToString(), StringComparison.Ordinal))
            {
                throw new InvalidOperationException("The custom id of the button must start with the id of the data.");
            }
            else if (!procrastinator.TryAddData(id, data))
            {
                throw new InvalidOperationException("The data could not be added to the dictionary.");
            }

            data.Message = await member.SendMessageAsync(new DiscordMessageBuilder()
                .WithAllowedMentions(Mentions.None)
                .WithContent(question)
                .AddActionRowComponent(button)
            );

            await data.TaskCompletionSource.Task;
            return data.TaskCompletionSource.Task.Result;
        }

        public static async ValueTask<string?> PromptAsync(this CommandContext context, string question, string placeholder, IPromptComponentCreator? componentCreator = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));
            ArgumentNullException.ThrowIfNull(placeholder, nameof(placeholder));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(question.Length, 45, nameof(question));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(placeholder.Length, 100, nameof(placeholder));

            Procrastinator procrastinator = context.ServiceProvider.GetRequiredService<Procrastinator>();
            componentCreator ??= procrastinator.Configuration.GetComponentCreatorOrDefault<IPromptComponentCreator, PromptDefaultComponentCreator>();

            Ulid id = Ulid.NewUlid();
            PromptMoment data = new()
            {
                Id = id,
                AuthorId = context.User.Id,
                CancellationToken = procrastinator.RegisterTimeoutCallback(id, cancellationToken),
                ComponentCreator = componentCreator,
                Question = question,
                Placeholder = placeholder
            };

            if (context is TextCommandContext textContext)
            {
                DiscordButtonComponent button = componentCreator.CreateTextPromptButton(question, id);
                if (!button.CustomId.StartsWith(id.ToString(), StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("The custom id of the button must start with the id of the data.");
                }
                else if (!procrastinator.TryAddData(id, data))
                {
                    throw new InvalidOperationException("The data could not be added to the dictionary.");
                }

                DiscordMessageBuilder builder = new DiscordMessageBuilder()
                    .WithAllowedMentions(Mentions.None)
                    .WithContent(question)
                    .AddActionRowComponent(button);

                await textContext.RespondAsync(builder);
                data.Message = textContext.Response;
            }
            else if (context is SlashCommandContext slashContext)
            {
                if (!procrastinator.TryAddData(id, data))
                {
                    throw new InvalidOperationException("The data could not be added to the dictionary.");
                }

                await slashContext.RespondWithModalAsync(new DiscordInteractionResponseBuilder()
                    .WithTitle(question)
                    .WithCustomId(id.ToString())
                    .AddTextInputComponent(componentCreator.CreateModalPromptButton(question, placeholder, id))
                );
            }
            else
            {
                throw new InvalidOperationException($"Unsupported context type: {context.GetType().Name}");
            }

            await data.TaskCompletionSource.Task;
            return data.TaskCompletionSource.Task.Result;
        }
    }
}
