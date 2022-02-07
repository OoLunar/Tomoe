using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomoe.Attributes;
using Tomoe.Models;

namespace Tomoe.Events
{
    public class AutoReactEvent
    {
        [SubscribeToEvent(nameof(DiscordShardedClient.MessageCreated))]
        public static async Task AutoReactAsync(DiscordClient client, MessageCreateEventArgs messageEventArgs)
        {
            DatabaseContext Database = Program.ServiceProvider.GetService<DatabaseContext>() ?? throw new InvalidOperationException("Unable to retrieve the Database.");
            IEnumerable<AutoReactionModel> autoReacts = Database.AutoReactions.Where(databaseAutoReact => databaseAutoReact.GuildId == messageEventArgs.Guild.Id && databaseAutoReact.ChannelId == messageEventArgs.Channel.Id);
            if (!autoReacts.Any())
            {
                return;
            }

            bool databaseEdited = false;

            if (databaseEdited)
            {
                await Database.SaveChangesAsync();
            }
        }
    }
}