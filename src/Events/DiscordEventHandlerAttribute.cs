using System;
using System.Linq;

namespace OoLunar.Tomoe.Attributes
{
    /// <summary>
    /// Subscribe to a Discord event.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class DiscordEventHandlerAttribute : Attribute
    {
        /// <summary>
        /// The event's type to subscribe to.
        /// </summary>
        public Type EventType { get; init; }

        /// <summary>
        /// The events that this method subscribes to.
        /// </summary>
        public string[] EventNames { get; init; }

        /// <summary>
        /// Subscribe to the events specified.
        /// </summary>
        /// <param name="eventNames">The event names to subscribe too.</param>
        public DiscordEventHandlerAttribute(Type eventType, params string[] eventNames)
        {
            ArgumentNullException.ThrowIfNull(eventType, nameof(eventType));
            if (eventNames == null || eventNames.Length == 0 || eventNames.Any(eventName => string.IsNullOrWhiteSpace(eventName)))
            {
                throw new ArgumentException("Value cannot be null, empty or contain whitespace values.", nameof(eventNames));
            }

            EventType = eventType;
            EventNames = eventNames;
        }
    }
}
