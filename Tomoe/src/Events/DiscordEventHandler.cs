using System;
using System.Reflection;

namespace OoLunar.Tomoe.Events
{
    public sealed class DiscordEventHandler
    {
        public Type EventType { get; init; }
        public string[] EventNames { get; init; }
        public MethodInfo EventHandler { get; init; }

        public DiscordEventHandler(Type eventType, string[] eventNames, MethodInfo eventHandler)
        {
            ArgumentNullException.ThrowIfNull(eventType, nameof(eventType));
            if (eventNames == null || eventNames.Length == 0)
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(eventNames));
            }
            ArgumentNullException.ThrowIfNull(eventHandler, nameof(eventHandler));

            EventType = eventType;
            EventNames = eventNames;
            EventHandler = eventHandler;
        }
    }
}
