using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using EdgeDB;
using Microsoft.Extensions.Configuration;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Services
{
    public sealed class DiscordGuildPrefixResolverService
    {
        private IConfiguration Configuration { get; init; }
        private EdgeDBClient EdgeDbClient { get; init; }
        private CancellationToken CancellationToken { get; init; }

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
            int mentionPrefix = message.GetMentionPrefixLength(message.Channel.Users.FirstOrDefault(user => user.IsCurrent) ?? message.Channel.Guild.CurrentMember);
            if (mentionPrefix != 1)
            {
                return mentionPrefix;
            }
            else if (message.Channel.Guild == null || EdgeDbClient == null)
            {
                return ResolvePrefixes(message, null);
            }
            else
            {
                CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));

                GuildModel guildModel = (await QueryBuilder.Select<GuildModel>().Filter(guild => guild.GuildId == message.Channel.Guild.Id).Limit(1).ExecuteAsync(EdgeDbClient, Capabilities.ReadOnly, cancellationTokenSource.Token)).FirstOrDefault() ?? throw new InvalidOperationException($"Guild {message.Channel.Guild.Id} not found in the database.");
                return ResolvePrefixes(message, guildModel.Prefixes.Select(prefix => prefix.Prefix));
            }
        }

        private int ResolvePrefixes(DiscordMessage message, IEnumerable<string>? prefixes = null)
        {
            if (prefixes == null || !prefixes.Any())
            {
                prefixes = Configuration.GetSection("discord:prefixes").Get<string[]>();
            }

            // Use the default prefixes
            foreach (string prefix in prefixes)
            {
                int prefixLength = message.GetStringPrefixLength(prefix);
                if (prefixLength != -1)
                {
                    return prefixLength;
                }
            }

            return -1;
        }
    }
}
