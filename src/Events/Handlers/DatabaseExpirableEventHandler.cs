using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Database;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class DatabaseExpirableEventHandler : IEventHandler<GuildDownloadCompletedEventArgs>
    {
        private readonly IServiceProvider _serviceProvider;
        public DatabaseExpirableEventHandler(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        [DiscordEvent(DiscordIntents.Guilds)]
        public Task HandleEventAsync(DiscordClient sender, GuildDownloadCompletedEventArgs eventArgs)
        {
            // Start all database expirable services
            foreach (FieldInfo field in typeof(Program).Assembly.GetTypes()
                // Grab all fields
                .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                // Ensure the field is of type DatabaseExpirableManager
                .Where(property => property.FieldType.IsGenericType && !property.FieldType.ContainsGenericParameters && property.FieldType.GetGenericTypeDefinition() == typeof(DatabaseExpirableManager<,>)))
            {
                _serviceProvider.GetRequiredService(field.FieldType);
            }

            return Task.CompletedTask;
        }
    }
}
