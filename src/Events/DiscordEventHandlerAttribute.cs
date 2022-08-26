using System;
using System.Linq;

namespace OoLunar.Tomoe.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class DiscordEventHandlerAttribute : Attribute
    {
        public string[] EventNames { get; init; }

        public DiscordEventHandlerAttribute(params string[] eventNames)
        {
            if (eventNames == null || eventNames.Length == 0 || eventNames.Any(eventName => string.IsNullOrWhiteSpace(eventName)))
            {
                throw new ArgumentException("Value cannot be null, empty or contain whitespace values.", nameof(eventNames));
            }

            EventNames = eventNames;
        }
    }
}
