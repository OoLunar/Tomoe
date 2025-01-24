using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.Tomoe.Interactivity.Moments.Pagination
{
    public static class ExtensionMethods
    {
        public static async ValueTask PaginateAsync(this CommandContext context, IReadOnlyList<Page> pages, IPaginationComponentCreator? componentCreator = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(pages, nameof(pages));

            Procrastinator procrastinator = context.ServiceProvider.GetRequiredService<Procrastinator>();
            componentCreator ??= procrastinator.Configuration.GetComponentCreatorOrDefault<IPaginationComponentCreator, PaginationDefaultComponentCreator>();

            Ulid id = Ulid.NewUlid();
            PaginationMoment data = new()
            {
                Id = id,
                AuthorId = context.User.Id,
                CancellationToken = procrastinator.RegisterTimeoutCallback(id, cancellationToken),
                ComponentCreator = componentCreator,
                Pages = pages
            };

            if (!procrastinator.TryAddData(id, data))
            {
                throw new InvalidOperationException("The data could not be added to the dictionary.");
            }

            DiscordMessageBuilder messageBuilder = pages[0].CreateMessage(data);
            await context.RespondAsync(messageBuilder);
            data.Message = await context.GetResponseAsync();
        }
    }
}
