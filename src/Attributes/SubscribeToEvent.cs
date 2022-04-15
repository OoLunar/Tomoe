using System;

namespace Tomoe.Attributes
{
    /// <summary>
    /// Subscribes to an event on the <see cref="DSharpPlus.DiscordShardedClient"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SubscribeToEventAttribute : Attribute
    {
        /// <summary>
        /// The <see cref="DSharpPlus.DiscordShardedClient"/> event name to subscribe to.
        /// </summary>
        public string EventName { get; init; }

        /// <summary>
        /// Subscribes to an event on the <see cref="DSharpPlus.DiscordShardedClient"/> instance. It's recommended to use <c>nameof(<see cref="DSharpPlus.DiscordShardedClient"/>.EventName)</c> when using the attribute.
        /// </summary>
        /// <param name="eventName">The event name to subscribe to.</param>
        public SubscribeToEventAttribute(string eventName) => EventName = eventName;
    }
}
