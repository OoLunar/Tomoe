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
    public sealed class DatabaseExpirableEventHandlers
    {
        private readonly IServiceProvider _serviceProvider;
        public DatabaseExpirableEventHandlers(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        [DiscordEvent(DiscordIntents.Guilds)]
        public Task OnGuildDownloadCompletedAsync(DiscordClient client, GuildDownloadCompletedEventArgs eventArgs)
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
