using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Interactivity.ComponentCreators;

namespace OoLunar.Tomoe.Interactivity
{
    public static class ExtensionMethods
    {
        public static async ValueTask<string?> PromptAsync(this CommandContext context, string question, IComponentCreator? componentCreator = null, CancellationToken cancellationToken = default) => await context.ServiceProvider.GetRequiredService<Procrastinator>().PromptAsync(context, question, componentCreator, cancellationToken);

        public static async ValueTask<string?> ChooseAsync(this CommandContext context, string question, IReadOnlyList<string> options, IComponentCreator? componentCreator = null, CancellationToken cancellationToken = default) => await context.ServiceProvider.GetRequiredService<Procrastinator>().ChooseAsync(context, question, options, componentCreator, cancellationToken);

        public static async ValueTask<IReadOnlyList<string>> ChooseMultipleAsync(this CommandContext context, string question, IReadOnlyList<string> options, IComponentCreator? componentCreator = null, CancellationToken cancellationToken = default) => await context.ServiceProvider.GetRequiredService<Procrastinator>().ChooseMultipleAsync(context, question, options, componentCreator, cancellationToken);

        public static async ValueTask<bool?> ConfirmAsync(this CommandContext context, string question, IComponentCreator? componentCreator = null, CancellationToken cancellationToken = default) => await context.ServiceProvider.GetRequiredService<Procrastinator>().ConfirmAsync(context, question, componentCreator, cancellationToken);

        public static IEnumerable<DiscordComponent> Mutate<T>(this IEnumerable<DiscordComponent> source, Predicate<T> filter, Func<T, T> mutation) where T : DiscordComponent
        {
            foreach (DiscordComponent component in source)
            {
                yield return component switch
                {
                    T target => filter(target) ? mutation(target) : target,
                    DiscordActionRowComponent actionRow => new DiscordActionRowComponent(actionRow.Components.Mutate(filter, mutation)),
                    _ => component
                };
            }
        }
    }
}
