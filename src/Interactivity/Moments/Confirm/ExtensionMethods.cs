using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.Tomoe.Interactivity.Moments.Confirm
{
    public static class ExtensionMethods
    {
        public static async ValueTask<bool?> ConfirmAsync(this CommandContext context, string question, IConfirmComponentCreator? componentCreator = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(question, nameof(question));

            Procrastinator procrastinator = context.ServiceProvider.GetRequiredService<Procrastinator>();
            componentCreator ??= procrastinator.Configuration.GetComponentCreatorOrDefault<IConfirmComponentCreator, ConfirmDefaultComponentCreator>();

            Ulid id = Ulid.NewUlid();
            ConfirmMoment data = new()
            {
                Id = id,
                AuthorId = context.User.Id,
                ComponentCreator = componentCreator,
                CancellationToken = procrastinator.RegisterTimeoutCallback(id, cancellationToken),
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

            if (!procrastinator.TryAddData(id, data))
            {
                throw new InvalidOperationException("The data could not be added to the dictionary.");
            }

            await context.RespondAsync(new DiscordMessageBuilder()
                .WithAllowedMentions(Mentions.None)
                .WithContent(question)
                .AddComponents(buttons)
            );

            data.Message = await context.GetResponseAsync();
            await data.TaskCompletionSource.Task;
            return data.TaskCompletionSource.Task.Result;
        }
    }
}
