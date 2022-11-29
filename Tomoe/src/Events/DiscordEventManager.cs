using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Attributes;
using OoLunar.Tomoe.Events;

namespace OoLunar.Tomoe
{
    /// <summary>
    /// Attempts to automatically register events and returns the required <see cref="DiscordIntents"/> from the registered events.
    /// </summary>
    public sealed class DiscordEventManager
    {
        /// <summary>
        /// An attempted mapping of which intents are required for which events.
        /// </summary>
        public static readonly IReadOnlyDictionary<DiscordIntents, string[]> EventIntentMappings = new ReadOnlyDictionary<DiscordIntents, string[]>(new Dictionary<DiscordIntents, string[]>()
        {
            // Map DiscordIntents to a dictionary using the event's nameof()
            [DiscordIntents.Guilds] = new[] { nameof(DiscordShardedClient.GuildCreated), nameof(DiscordShardedClient.GuildUpdated), nameof(DiscordShardedClient.GuildDeleted), nameof(DiscordShardedClient.GuildRoleCreated), nameof(DiscordShardedClient.GuildRoleUpdated), nameof(DiscordShardedClient.GuildRoleDeleted), nameof(DiscordShardedClient.ChannelCreated), nameof(DiscordShardedClient.ChannelUpdated), nameof(DiscordShardedClient.ChannelDeleted), nameof(DiscordShardedClient.ChannelPinsUpdated), nameof(DiscordShardedClient.ThreadCreated), nameof(DiscordShardedClient.ThreadUpdated), nameof(DiscordShardedClient.ThreadDeleted), nameof(DiscordShardedClient.ThreadListSynced), nameof(DiscordShardedClient.ThreadMemberUpdated), nameof(DiscordShardedClient.ThreadMembersUpdated), nameof(DiscordShardedClient.StageInstanceCreated), nameof(DiscordShardedClient.StageInstanceUpdated), nameof(DiscordShardedClient.StageInstanceDeleted) },
            [DiscordIntents.GuildMembers] = new[] { nameof(DiscordShardedClient.GuildMemberAdded), nameof(DiscordShardedClient.GuildMemberRemoved), nameof(DiscordShardedClient.GuildMemberUpdated), nameof(DiscordShardedClient.ThreadMemberUpdated) },
            [DiscordIntents.GuildBans] = new[] { nameof(DiscordShardedClient.GuildBanAdded), nameof(DiscordShardedClient.GuildBanRemoved) },
            [DiscordIntents.GuildEmojis] = new[] { nameof(DiscordShardedClient.GuildEmojisUpdated), nameof(DiscordShardedClient.GuildStickersUpdated) },
            [DiscordIntents.GuildIntegrations] = new[] { nameof(DiscordShardedClient.GuildIntegrationsUpdated), nameof(DiscordShardedClient.IntegrationCreated), nameof(DiscordShardedClient.IntegrationUpdated), nameof(DiscordShardedClient.IntegrationDeleted) },
            [DiscordIntents.GuildWebhooks] = new[] { nameof(DiscordShardedClient.WebhooksUpdated) },
            [DiscordIntents.GuildInvites] = new[] { nameof(DiscordShardedClient.InviteCreated), nameof(DiscordShardedClient.InviteDeleted) },
            [DiscordIntents.GuildVoiceStates] = new[] { nameof(DiscordShardedClient.VoiceStateUpdated) },
            [DiscordIntents.GuildPresences] = new[] { nameof(DiscordShardedClient.PresenceUpdated) },
            [DiscordIntents.GuildMessages] = new[] { nameof(DiscordShardedClient.MessageCreated), nameof(DiscordShardedClient.MessageUpdated), nameof(DiscordShardedClient.MessageDeleted), nameof(DiscordShardedClient.MessagesBulkDeleted) },
            [DiscordIntents.GuildMessageReactions] = new[] { nameof(DiscordShardedClient.MessageReactionAdded), nameof(DiscordShardedClient.MessageReactionRemoved), nameof(DiscordShardedClient.MessageReactionRemovedEmoji), nameof(DiscordShardedClient.MessageReactionsCleared) },
            [DiscordIntents.GuildMessageTyping] = new[] { nameof(DiscordShardedClient.TypingStarted) },
            [DiscordIntents.DirectMessages] = new[] { nameof(DiscordShardedClient.MessageCreated), nameof(DiscordShardedClient.MessageUpdated), nameof(DiscordShardedClient.MessageDeleted), nameof(DiscordShardedClient.ChannelPinsUpdated) },
            [DiscordIntents.DirectMessageReactions] = new[] { nameof(DiscordShardedClient.MessageReactionAdded), nameof(DiscordShardedClient.MessageReactionRemoved), nameof(DiscordShardedClient.MessageReactionRemovedEmoji), nameof(DiscordShardedClient.MessageReactionsCleared) },
            [DiscordIntents.DirectMessageTyping] = new[] { nameof(DiscordShardedClient.TypingStarted) }
        });

        /// <summary>
        /// The event handler methods to register to the events.
        /// </summary>
        public DiscordEventHandler[] EventHandlers { get; init; }

        /// <summary>
        /// The <see cref="IServiceProvider"/> to use when creating the event handler's class. The class is only created once before being registered to the events.
        /// </summary>
        public IServiceProvider ServiceProvider { get; init; }

        public DiscordEventManager(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            ServiceProvider = serviceProvider;
            List<DiscordEventHandler> eventHandlers = new();

            // Find all the types that have the DiscordEventHandler attribute
            foreach (Type type in typeof(Program).Assembly.GetTypes())
            {
                // The attribute can only be applied to methods
                foreach (MethodInfo method in type.GetRuntimeMethods())
                {
                    // Little syntax sugar to test if the attribute is not null.
                    if (method.GetCustomAttribute<DiscordEventHandlerAttribute>() is not DiscordEventHandlerAttribute eventHandlerAttribute)
                    {
                        continue;
                    }

                    // Register the method to the event handlers.
                    eventHandlers.Add(new DiscordEventHandler(eventHandlerAttribute.EventType, eventHandlerAttribute.EventNames, method));
                }
            }

            // ToArray because we're going to enumerate it multiple times and we need the total element count.
            EventHandlers = eventHandlers.ToArray();
        }

        /// <summary>
        /// Subscribe the event handlers to the events found on the object.
        /// </summary>
        /// <param name="obj">The object to register the events too.</param>
        public void Subscribe(object obj)
        {
            // Iterate through the event handlers
            foreach (DiscordEventHandler eventHandler in EventHandlers)
            {
                // Skip the event handlers that don't match the object's type
                if (!eventHandler.EventType.IsAssignableFrom(obj.GetType()))
                {
                    continue;
                }

                // Attempt to register the event handler to the events
                foreach (string eventName in eventHandler.EventNames)
                {
                    EventInfo? eventInfo = eventHandler.EventType.GetEvent(eventName);
                    if (eventInfo is null)
                    {
                        throw new ArgumentException($"The event {eventName} was not found on the type {eventHandler.EventType.Name}.");
                    }
                    else if (!eventInfo.EventHandlerType!.GetGenericArguments().SequenceEqual(eventHandler.EventHandler.GetParameters().Select(parameter => parameter.ParameterType)))
                    {
                        throw new ArgumentException($"The event {eventName} on the type {eventHandler.EventType.Name} does not have the same parameters as the event handler {eventHandler.EventHandler.Name}.");
                    }
                    else if (eventHandler.EventHandler.IsStatic)
                    {
                        // Static method, no injection
                        eventInfo.AddEventHandler(obj, Delegate.CreateDelegate(eventInfo.EventHandlerType!, eventHandler.EventHandler.DeclaringType!, eventHandler.EventHandler.Name));
                    }
                    else if (eventHandler.EventHandler.DeclaringType?.GetConstructors()[0].GetParameters().Length != 0)
                    {
                        // Constructor injection
                        eventInfo.AddEventHandler(obj, Delegate.CreateDelegate(eventInfo.EventHandlerType!, ActivatorUtilities.CreateInstance(ServiceProvider, eventHandler.EventHandler.DeclaringType!), eventHandler.EventHandler.Name));
                    }
                    else
                    {
                        IEnumerable<PropertyInfo> properties = eventHandler.EventHandler.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public).Where(property => property.GetCustomAttribute<DontInjectAttribute>() == null);
                        if (!properties.Any())
                        {
                            // Plain object, no injection
                            eventInfo.AddEventHandler(obj, Delegate.CreateDelegate(eventInfo.EventHandlerType!, Activator.CreateInstance(eventHandler.EventHandler.DeclaringType!)!, eventHandler.EventHandler.Name));
                        }
                        else
                        {
                            // Property injection
                            object instance = Activator.CreateInstance(eventHandler.EventHandler.DeclaringType!)!;
                            foreach (PropertyInfo property in properties)
                            {
                                property.SetValue(instance, ServiceProvider.GetService(property.PropertyType));
                            }
                            eventInfo.AddEventHandler(obj, Delegate.CreateDelegate(eventInfo.EventHandlerType!, instance, eventHandler.EventHandler.Name));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Grabs the required intents from the event handlers.
        /// </summary>
        /// <returns>The <see cref="DiscordIntents"/> required to register to the events.</returns>
        public DiscordIntents GetIntents()
        {
            DiscordIntents intents = 0;
            if (EventHandlers.Length > 0)
            {
                // Iterate through the mappings
                foreach ((DiscordIntents intent, string[] eventNames) in EventIntentMappings)
                {
                    // Iterate through the event handlers
                    foreach (DiscordEventHandler eventHandler in EventHandlers)
                    {
                        // Test if the event handler's names matches with any of the event names in the mapping
                        if (eventHandler.EventNames.Intersect(eventNames).Any())
                        {
                            // Add that event's required intent.
                            intents |= intent;
                        }
                    }
                }
            }
            return intents;
        }
    }
}
