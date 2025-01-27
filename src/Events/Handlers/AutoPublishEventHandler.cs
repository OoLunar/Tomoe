using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class AutoPublishEventHandler : IEventHandler<MessageCreatedEventArgs>
    {
        [DiscordEvent(DiscordIntents.GuildMessages)]
        public async Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs)
        {
            if (eventArgs.Channel.Type is DiscordChannelType.News && await AutoPublishModel.ExistsAsync(eventArgs.Guild.Id, eventArgs.Channel.Id))
            {
                await eventArgs.Channel.CrosspostMessageAsync(eventArgs.Message);
            }
        }
    }
}
