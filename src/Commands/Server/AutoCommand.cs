using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Tomoe.Enums;
using Tomoe.Models;

namespace Tomoe.Commands.Server
{
    public abstract class AutoCommand<T> : BaseCommandModule where T : class?
    {
        public DatabaseContext Database { get; init; } = null!;
        public Logger<AutoCommand<T>> Logger { get; init; } = null!;

        public virtual Task AddAsync(CommandContext context, DiscordChannel channel, params T[] value) => AddAsync(context, channel, FilterType.AllMessages, null, value);
        public virtual async Task AddAsync(CommandContext context, DiscordChannel channel, FilterType filterType, string? filter, params T[] value)
        {
#pragma warning disable IDE0072 // Populate switch. Compiler bug: https://github.com/dotnet/roslyn/issues/58468
            string? filterRegex = filterType switch
            {
                FilterType.Regex => filter,
                FilterType.Phrase => filter,
                FilterType.Ping => @"\<@&?!?\d+\>",
                FilterType.RolePing => @"\<@&\d+\>",
                FilterType.UserPing => @"\<@!?\d+\>",
                _ => null
            };

            bool requiresUserFilter = filterType switch
            {
                FilterType.Regex => true,
                FilterType.Phrase => true,
                _ => false
            };
#pragma warning restore IDE0072 // Populate switch

            if (!requiresUserFilter && filter != null) // In case if the filter type doesn't require a filter, and the filter is supposed to be T
            {
                try
                {
                    if (await context.CommandsNext.ConvertArgument<T>(filter, context) is T t)
                    {
                        value = value.Append(t).ToArray();
                    }
                }
                catch (Exception)
                {
                    Logger.LogWarning("Failed to convert filter to type {Type}", typeof(T));
                }
            }

            AutoModel<T> autoModel = new()
            {
                GuildId = context.Guild.Id,
                ChannelId = channel.Id,
                FilterType = filterType,
                Filter = filterRegex,
                Values = new List<T>(value)
            };
            Database.Set<AutoModel<T>>().Add(autoModel);
            await Database.SaveChangesAsync();
        }

        public virtual async Task RemoveAsync(CommandContext context, params Guid[] ids)
        {
            Database.Set<AutoModel<T>>().RemoveRange(Database.Set<AutoModel<T>>().Where(x => ids.Contains(x.Id)));
            await Database.SaveChangesAsync();
        }

        public virtual async Task RemoveAsync(CommandContext context, params DiscordChannel[] channels)
        {
            IEnumerable<ulong>? channelIds = channels.Select(x => x.Id);
            Database.Set<AutoModel<T>>().RemoveRange(Database.Set<AutoModel<T>>().Where(x => channelIds.Contains(x.ChannelId)));
            await Database.SaveChangesAsync();
        }

        public virtual async Task RemoveAsync(CommandContext context, params KeyValuePair<FilterType, string?>[] filterTypes)
        {
            Database.Set<AutoModel<T>>().RemoveRange(Database.Set<AutoModel<T>>().Where(x => filterTypes.Any(y => y.Key == x.FilterType && y.Value == x.Filter)));
            await Database.SaveChangesAsync();
        }

        public virtual async Task RemoveAsync(CommandContext context, params T[] values)
        {
            Database.Set<AutoModel<T>>().RemoveRange(Database.Set<AutoModel<T>>().Where(x => x.Values.SequenceEqual(values)));
            await Database.SaveChangesAsync();
        }

        //Task ListAsync(CommandContext context);
        //Task ListAsync(CommandContext context, params Guid[] ids);
        //Task ListAsync(CommandContext context, params DiscordChannel[] channels);
        //Task ListAsync(CommandContext context, params KeyValuePair<FilterType, string?>[] filterTypes);
        //Task ListAsync(CommandContext context, T value);
    }
}