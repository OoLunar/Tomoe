using System;
using System.Reflection;

namespace OoLunar.Tomoe.Events
{
    public sealed class DiscordEventHandler
    {
        public string[] EventNames { get; init; }
        public MethodInfo EventHandler { get; init; }

        public DiscordEventHandler(string[] eventNames, MethodInfo eventHandler)
        {
            if (eventNames == null || eventNames.Length == 0)
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(eventNames));
            }
            ArgumentNullException.ThrowIfNull(eventHandler, nameof(eventHandler));

            EventNames = eventNames;
            EventHandler = eventHandler;
        }
    }
}
