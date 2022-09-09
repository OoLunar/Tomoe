using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeDB;
using Microsoft.Extensions.Caching.Memory;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Services
{
    public sealed class GuildModelResolverService
    {
        /// <summary>
        /// Execute queries against the database.
        /// </summary>
        private EdgeDBClient EdgeDBClient { get; init; }

        /// <summary>
        /// Cache the models by their guild id.
        /// </summary>
        private MemoryCache GuildModelCache { get; init; }

        /// <summary>
        /// For cancelling the operation.
        /// </summary>
        private CancellationToken CancellationToken { get; init; }

        public GuildModelResolverService(EdgeDBClient edgeDBClient, MemoryCache memoryCache, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(edgeDBClient, nameof(edgeDBClient));
            ArgumentNullException.ThrowIfNull(memoryCache, nameof(memoryCache));

            EdgeDBClient = edgeDBClient;
            GuildModelCache = memoryCache;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Attempt to grab the cached guild model or query it from the database.
        /// </summary>
        /// <param name="guildId">The guild to fetch.</param>
        /// <returns><see langword="null"/> if the guild doesn't exist.</returns>
        public async Task<GuildModel?> GetAsync(ulong guildId)
        {
            if (!GuildModelCache.TryGetValue(guildId, out GuildModel? guildModel))
            {
                guildModel = (await EdgeDBClient.QueryAsync<GuildModel?>("SELECT GuildModel FILTER .id = $guildId", new Dictionary<string, object?>() { ["guildId"] = guildId }, Capabilities.ReadOnly, CancellationToken)).FirstOrDefault();
                if (guildModel != null)
                {
                    // Cache the guild model for 5 minutes, resetting the expire time in the process.
                    GuildModelCache.Set(guildId, guildModel, TimeSpan.FromMinutes(5));
                }
            }

            return guildModel;
        }

        /// <summary>
        /// Creates a new guild model in the database.
        /// </summary>
        /// <param name="guildModel">The guild model to insert.</param>
        /// <returns>The created guild model with updated information from the database.</returns>
        public async Task<GuildModel> InsertAsync(GuildModel guildModel)
        {
            ArgumentNullException.ThrowIfNull(guildModel, nameof(guildModel));

            GuildModel? dbGuildModel = await QueryBuilder.Insert(guildModel).ExecuteAsync(EdgeDBClient, Capabilities.Modifications, CancellationToken);
            if (dbGuildModel == null)
            {
                throw new InvalidOperationException("Insert returned null.");
            }

            GuildModelCache.Set(dbGuildModel.GuildId, dbGuildModel, TimeSpan.FromMinutes(5));
            return dbGuildModel;
        }
    }
}
