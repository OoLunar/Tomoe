using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.Tomoe.Events
{
    public sealed class DiscordEventManager
    {
        public DiscordIntents Intents { get; private set; } = TextCommandProcessor.RequiredIntents | SlashCommandProcessor.RequiredIntents | DiscordIntents.MessageContents;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<MethodInfo> _eventHandlers = [];

        public DiscordEventManager(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public void GatherEventHandlers(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));
            foreach (Type type in assembly.GetExportedTypes())
            {
                foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (methodInfo.GetCustomAttribute<DiscordEventAttribute>() is DiscordEventAttribute eventAttribute)
                    {
                        Intents |= eventAttribute.Intents;
                        _eventHandlers.Add(methodInfo);
                    }
                }
            }
        }

        public void RegisterEventHandlers(DiscordClientBuilder clientBuilder)
        {
            ArgumentNullException.ThrowIfNull(clientBuilder, nameof(clientBuilder));
            clientBuilder.ConfigureEventHandlers(eventHandlingBuilder =>
            {
                foreach (MethodInfo methodInfo in typeof(EventHandlingBuilder).GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!methodInfo.Name.StartsWith("Handle", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    foreach (MethodInfo eventHandler in _eventHandlers)
                    {
                        if (methodInfo.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(eventHandler.GetParameters().Select(parameter => parameter.ParameterType)))
                        {
                            eventHandlingBuilder = (EventHandlingBuilder)methodInfo.Invoke(eventHandlingBuilder, [eventHandler])!;
                        }
                    }
                }
            });
        }

        public void RegisterEventHandlers(object obj)
        {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
            foreach (EventInfo eventInfo in obj.GetType().GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                foreach (MethodInfo methodInfo in _eventHandlers)
                {
                    if (eventInfo.EventHandlerType!.GetGenericArguments().SequenceEqual(methodInfo.GetParameters().Select(parameter => parameter.ParameterType)))
                    {
                        Delegate handler = methodInfo.IsStatic
                            ? Delegate.CreateDelegate(eventInfo.EventHandlerType, methodInfo)
                            : Delegate.CreateDelegate(eventInfo.EventHandlerType, ActivatorUtilities.CreateInstance(_serviceProvider, methodInfo.DeclaringType!), methodInfo);

                        eventInfo.AddEventHandler(obj, handler);
                    }
                }
            }
        }
    }
}
