using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeDB;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Services
{
    // Genuinely unsure if this could be replaced with DatabaseTracker
    public sealed class AuditService
    {
        private EdgeDBClient EdgeDBClient { get; init; }
        private CancellationToken CancellationToken { get; init; }

        public AuditService(EdgeDBClient edgeDBClient, CancellationToken cancellationToken)
        {
            EdgeDBClient = edgeDBClient;
            CancellationToken = cancellationToken;
        }

        public Task AddAsync(AuditableCommand auditable, ulong guildId) => EdgeDBClient.ExecuteAsync(@"
        INSERT INTO audit (
            command_name := $commandName
            guild := (SELECT Guild FILTER .id = $guildId),
            authorizer := (SELECT GuildMember FILTER .id = $authorizerId AND .guild = $guildId),
            affected_users := $affectedUsers,
            reason := $reason,
            successful := $successful,
            notes := $notes,
            duration_length := $durationLength,
        ) UNLESS CONFLICT ON .audit_id;", new Dictionary<string, object?>()
        {
            ["commandName"] = auditable.GetType().Name.Split("Command")[0],
            ["guildId"] = guildId,
            ["authorizerId"] = auditable.Audit.Authorizer.Id,
            ["affectedUsers"] = auditable.Audit.AffectedUsers,
            ["reason"] = auditable.Audit.Reason,
            ["successful"] = auditable.Audit.Successful,
            ["notes"] = auditable.Audit.Notes,
            ["durationLength"] = auditable.Audit.Duration,
        }, Capabilities.Modifications, CancellationToken);

        public Task<IReadOnlyCollection<AuditableCommand?>> GetAsync(ulong guildId, int page = 1) => QueryBuilder
            .Select<AuditableCommand>()
            .Filter(x => x.Audit.Guild.GuildId == guildId)
            .Limit(10)
            .Offset(10 * (page - 1))
            .ExecuteAsync(EdgeDBClient, Capabilities.ReadOnly, CancellationToken);

        public async Task<AuditableCommand?> GetAsync(Guid auditId) => (await QueryBuilder
            .Select<AuditableCommand>()
            .Filter(x => x.Audit.Id == auditId)
            .ExecuteAsync(EdgeDBClient, Capabilities.ReadOnly, CancellationToken)).FirstOrDefault();

        public async Task<int> GetCountAsync(ulong guildId) => (await EdgeDBClient.QueryAsync<int>(@"
        SELECT COUNT(*)
        FROM audit
        WHERE guild = (SELECT Guild FILTER .id = $guildId);", new Dictionary<string, object?>()
        {
            ["guildId"] = guildId,
        }, Capabilities.ReadOnly, CancellationToken)).FirstOrDefault();
    }
}
