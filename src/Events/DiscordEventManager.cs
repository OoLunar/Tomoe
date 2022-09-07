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
    public sealed class DiscordEventManager
    {
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
        public DiscordEventHandler[] EventHandlers { get; init; }
        public IServiceProvider ServiceProvider { get; init; }

        public DiscordEventManager(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            ServiceProvider = serviceProvider;

            List<DiscordEventHandler> eventHandlers = new();
            foreach (Type type in typeof(Program).Assembly.GetTypes())
            {
                if (!type.IsClass)
                {
                    continue;
                }

                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.GetCustomAttribute<DiscordEventHandlerAttribute>() is not DiscordEventHandlerAttribute eventHandlerAttribute)
                    {
                        continue;
                    }
                    eventHandlers.Add(new DiscordEventHandler(eventHandlerAttribute.EventNames, method));
                }
            }

            EventHandlers = eventHandlers.ToArray();
        }

        public void Subscribe(object obj)
        {
            foreach (EventInfo eventInfo in obj.GetType().GetEvents())
            {
                foreach (DiscordEventHandler eventHandler in EventHandlers)
                {
                    if (eventHandler.EventNames.Contains(eventInfo.Name))
                    {
                        if (eventHandler.EventHandler.IsStatic)
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
        }

        public DiscordIntents GetIntents()
        {
            DiscordIntents intents = 0;
            if (EventHandlers.Length > 0)
            {
                foreach ((DiscordIntents intent, string[] eventNames) in EventIntentMappings)
                {
                    foreach (DiscordEventHandler eventHandler in EventHandlers)
                    {
                        if (eventHandler.EventNames.Intersect(eventNames).Any())
                        {
                            intents |= intent;
                        }
                    }
                }
            }
            return intents;
        }
    }
}
