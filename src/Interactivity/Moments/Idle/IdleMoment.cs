using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity.Moments.Idle
{
    public abstract record IdleMoment
    {
        public required Ulid Id { get; init; }
        public required ulong AuthorId { get; init; }
        public required CancellationToken CancellationToken { get; set; }
        public DiscordMessage? Message { get; set; }

        public abstract ValueTask HandleAsync(Procrastinator procrastinator, DiscordInteraction interaction);
        public virtual async ValueTask TimedOutAsync(Procrastinator procrastinator)
        {
            // If there's a message attached, disable all components related to the data
            if (Message is not null
                && Message.Components is not null
                && Message.Components.Count != 0)
            {
                DiscordMessageBuilder messageBuilder = new(Message);

                // Clear the components cause we're going to copy them over.
                // Copying is necessary due to DisableComponent's behavior, see impl.
                messageBuilder.ClearComponents();
                foreach (DiscordComponent component in DisableComponents(Id.ToString(), Message.Components))
                {
                    // We do this because ARC has it's own overload
                    if (component is DiscordActionRowComponent row)
                    {
                        messageBuilder.AddActionRowComponent(row);
                    }
                    else if (component is DiscordSelectComponent selectComponent)
                    {
                        messageBuilder.AddActionRowComponent(selectComponent);
                    }
                    else
                    {
                        throw new NotImplementedException($"The component type {component.GetType()} is not implemented.");
                    }
                }

                // Disable the component
                await Message.ModifyAsync(messageBuilder);
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
                    yield return new DiscordSelectComponent(select.CustomId, select.Placeholder, select.Options, true, select.MinimumSelectedValues ?? 1, select.MaximumSelectedValues ?? 1);
                }
            }
        }
    }
}
