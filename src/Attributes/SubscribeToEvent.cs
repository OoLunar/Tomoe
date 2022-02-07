using System;

namespace Tomoe.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SubscribeToEventAttribute : Attribute
    {
        public string EventName { get; init; }

        public SubscribeToEventAttribute(string eventName) => EventName = eventName;
    }
}