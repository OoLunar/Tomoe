using System;
using System.Reflection;
using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using Microsoft.Extensions.Logging;

namespace OoLunar.Tomoe.Events
{
    public sealed class DiscordIntentManager
    {
        public DiscordIntents Intents { get; private set; } = TextCommandProcessor.RequiredIntents | SlashCommandProcessor.RequiredIntents | DiscordIntents.MessageContents;
        private readonly ILogger<DiscordIntentManager> _logger;

        public DiscordIntentManager(ILogger<DiscordIntentManager> logger) => _logger = logger;

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
                        _logger.LogInformation("Event Handler {EventHandler} requests {Intents}", methodInfo.Name, eventAttribute.Intents);
                    }
                }
            }
        }
    }
}
