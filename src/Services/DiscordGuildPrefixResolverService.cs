using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using EdgeDB;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Services
{
    /// <summary>
    /// Attempts to resolve the prefix for a given guild from <see cref="DiscordMessage.Content"/>. Additionally caches the resolved prefixes for a given guild.
    /// </summary>
    public sealed class DiscordGuildPrefixResolverService
    {
        /// <summary>
        /// Used to access the default prefixes for the bot.
        /// </summary>
        private IConfiguration Configuration { get; init; }

        /// <summary>
        /// Used to access the database.
        /// </summary>
        private EdgeDBClient EdgeDbClient { get; init; }

        /// <summary>
        /// For cancelling the operation.
        /// </summary>
        private CancellationToken CancellationToken { get; init; }

        /// <summary>
        /// Caches the resolved prefixes for a given guild. Because the class is a <see cref="Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton"/>, we can init the cache here instead of requesting it from the <see cref="Microsoft.Extensions.DependencyInjection.IServiceProvider"/>.
        /// </summary>
        /// <remarks>
        /// Key: Guild ID
        /// Value: <see cref="List{string}"/> of prefixes
        /// </remarks>
        private MemoryCache GuildPrefixCache { get; init; } = new(new MemoryCacheOptions());

        public DiscordGuildPrefixResolverService(IConfiguration configuration, EdgeDBClient edgeDBClient, CancellationTokenSource cancellationTokenSource)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            ArgumentNullException.ThrowIfNull(edgeDBClient, nameof(edgeDBClient));

            Configuration = configuration;
            EdgeDbClient = edgeDBClient;
            CancellationToken = cancellationTokenSource.Token;
        }

        public async Task<int> ResolveAsync(DiscordMessage message)
        {
            // Mention prefix, always enabled.
            // If FirstOrDefault can't find anything, that means it's a VC and in a guild.
            int prefixLength = message.GetMentionPrefixLength(message.Channel.Users.FirstOrDefault(user => user.IsCurrent) ?? message.Channel.Guild.CurrentMember);
            if (prefixLength != 1)
            {
                return prefixLength;
            }
            else if (GuildPrefixCache.TryGetValue(message.Channel.GuildId, out List<string> cachedPrefixes) || message.Channel.Guild == null || EdgeDbClient == null)
            {
                // Attempt to resolve from the default prefixes from the config.
                return ResolvePrefixes(message, null);
            }
            // Assigning variables actually returns the value
            // We check if the message content starts with any of the prefixes, and if so, returns the prefix length. If it doesn't, we return -1 through the null coalescing operator.
            else if ((prefixLength = cachedPrefixes.FirstOrDefault(prefix => message.Content.StartsWith(prefix))?.Length ?? -1) != -1)
            {
                return prefixLength;
            }
            else
            {
                // Link the cancellation source with the parent one with a 5 second timeout.
                CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));

                // Select the guild model from the database, then grab it's prefixes.
                GuildModel guildModel = (await QueryBuilder.Select<GuildModel>().Filter(guild => guild.GuildId == message.Channel.GuildId).Limit(1).ExecuteAsync(EdgeDbClient, Capabilities.ReadOnly, cancellationTokenSource.Token)).FirstOrDefault() ?? throw new InvalidOperationException($"Guild {message.Channel.GuildId} not found in the database.");
                return ResolvePrefixes(message, guildModel.Prefixes.Select(prefix => prefix.Prefix));
            }
        }

        /// <summary>
        /// Attempts to give the prefix length for a given message, given a list of prefixes.
        /// </summary>
        /// <param name="message">The message to compare with.</param>
        /// <param name="prefixes">The prefixes to test with.</param>
        /// <returns>The length of the prefix that matched.</returns>
        private int ResolvePrefixes(DiscordMessage message, IEnumerable<string>? prefixes = null)
        {
            // Is null or empty but for IEnumerable
            if (prefixes == null || !prefixes.Any())
            {
                prefixes = Configuration.GetSection("discord:prefixes").Get<string[]>();
            }

            // Use the default prefixes from the config if the guild doesn't have any.
            foreach (string prefix in prefixes)
            {
                int prefixLength = message.GetStringPrefixLength(prefix);
                if (prefixLength != -1)
                {
                    // Append the prefix to the cache. Create a new entry if it doesn't exist.
                    GuildPrefixCache.GetOrCreate(message.Channel.GuildId, entry =>
                    {
                        // Automatically reset the expiration time if the entry is accessed.
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                        entry.Value = ((IEnumerable<string>)entry.Value).Append(prefix);
                        return entry;
                    });
                    return prefixLength;
                }
            }

            // No prefix matched.
            return -1;
        }
    }
}
