using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.Tomoe.Interactivity.Moments.Pagination
{
    public static class ExtensionMethods
    {
        public static async ValueTask PaginateAsync(this CommandContext context, IEnumerable<Page> pages, IPaginationComponentCreator? componentCreator = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(pages, nameof(pages));

            List<Page> pagesList = pages.ToList();
            if (pagesList.Count == 1)
            {
                await context.RespondAsync(pagesList[0].Message);
                return;
            }

            Procrastinator procrastinator = context.ServiceProvider.GetRequiredService<Procrastinator>();
            componentCreator ??= procrastinator.Configuration.GetComponentCreatorOrDefault<IPaginationComponentCreator, PaginationDefaultComponentCreator>();

            Ulid id = Ulid.NewUlid();
            PaginationMoment data = new()
            {
                Id = id,
                AuthorId = context.User.Id,
                CancellationToken = procrastinator.RegisterTimeoutCallback(id, cancellationToken),
                ComponentCreator = componentCreator,
                Pages = pagesList
            };

            if (!procrastinator.TryAddData(id, data))
            {
                throw new InvalidOperationException("The data could not be added to the dictionary.");
            }

            DiscordMessageBuilder messageBuilder = pagesList[0].CreateMessage(data);
            await context.RespondAsync(messageBuilder);
            data.Message = await context.GetResponseAsync();
        }
    }
}
