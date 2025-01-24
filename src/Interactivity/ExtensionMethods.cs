using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity
{
    public static class ExtensionMethods
    {
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
