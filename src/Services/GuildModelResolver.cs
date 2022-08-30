using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeDB;
using Microsoft.Extensions.Caching.Memory;
using OoLunar.Tomoe.Database;

namespace OoLunar.Tomoe.Services
{
    public sealed class GuildModelResolver
    {
        private EdgeDBClient EdgeDBClient { get; init; }
        private MemoryCache GuildModelCache { get; init; }
        private CancellationToken CancellationToken { get; init; }

        public GuildModelResolver(EdgeDBClient edgeDBClient, MemoryCache memoryCache, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(edgeDBClient, nameof(edgeDBClient));
            ArgumentNullException.ThrowIfNull(memoryCache, nameof(memoryCache));

            EdgeDBClient = edgeDBClient;
            GuildModelCache = memoryCache;
            CancellationToken = cancellationToken;
        }

        public async Task<GuildModel?> GetAsync(ulong guildId)
        {
            if (!GuildModelCache.TryGetValue(guildId, out GuildModel? guildModel))
            {
                guildModel = (await EdgeDBClient.QueryAsync<GuildModel?>("SELECT GuildModel FILTER .id = $guildId", new Dictionary<string, object?>() { ["guildId"] = guildId }, Capabilities.ReadOnly, CancellationToken)).FirstOrDefault();
                if (guildModel != null)
                {
                    GuildModelCache.Set(guildId, guildModel, TimeSpan.FromMinutes(5));
                }
            }

            return guildModel;
        }
    }
}
