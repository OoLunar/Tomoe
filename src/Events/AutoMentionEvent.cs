using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tomoe.Attributes;
using Tomoe.Models;

namespace Tomoe.Events
{
    public class AutoMentionEvent
    {
        [SubscribeToEvent(nameof(DiscordShardedClient.MessageCreated))]
        public static async Task AutoMentionAsync(DiscordClient client, MessageCreateEventArgs messageEventArgs)
        {
            if (messageEventArgs.Author.IsBot)
            {
                return;
            }

            DatabaseContext Database = Program.ServiceProvider.GetService<DatabaseContext>() ?? throw new InvalidOperationException("Unable to retrieve the Database.");
            IEnumerable<AutoMentionModel> autoMentions = Database.AutoMentions.Where(databaseAutoMention => databaseAutoMention.GuildId == messageEventArgs.Guild.Id && databaseAutoMention.ChannelId == messageEventArgs.Channel.Id);
            if (!autoMentions.Any())
            {
                return;
            }

            foreach (AutoMentionModel autoMention in autoMentions)
            {
                if (autoMention.Regex != null)
                {
                    if (!Regex.IsMatch(messageEventArgs.Message.Content, autoMention.Regex))
                    {
                        continue;
                    }
                }

                if (autoMention.IsRole)
                {
                    await messageEventArgs.Message.RespondAsync(new DiscordMessageBuilder().WithContent($"<@&{autoMention.Snowflake}>").WithAllowedMention(new RoleMention(autoMention.Snowflake)));
                }
                else
                {
                    await messageEventArgs.Message.RespondAsync(new DiscordMessageBuilder().WithContent($"<@{autoMention.Snowflake}>").WithAllowedMention(new UserMention(autoMention.Snowflake)));
                }
            }
        }
    }
}