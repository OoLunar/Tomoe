using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using OoLunar.Tomoe.Configuration;
using DiscordConfiguration = OoLunar.Tomoe.Configuration.DiscordConfiguration;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class ReadyEventHandler : IEventHandler<SessionCreatedEventArgs>
    {
        private readonly DiscordConfiguration _config;

        public ReadyEventHandler(TomoeConfiguration config) => _config = config.Discord;

        [DiscordEvent(DiscordIntents.None)]
        public async Task HandleEventAsync(DiscordClient client, SessionCreatedEventArgs eventArgs)
            => await client.UpdateStatusAsync(new DiscordActivity(_config.StatusText, _config.StatusType));
    }
}
