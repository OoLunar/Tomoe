using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeDB;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Services
{
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
            audit_id := $auditId,
            command_name := $commandName
            guild := (SELECT Guild FILTER .id = $guildId),
            authorizer := $authorizer,
            affected_users := $affectedUsers,
            reason := $reason,
        ) UNLESS CONFLICT ON .audit_id;", new Dictionary<string, object?>()
        {
            ["auditId"] = auditable.AuditId,
            ["commandName"] = auditable.GetType().Name.Split("Command")[0],
            ["guildId"] = guildId,
            ["authorizer"] = auditable.Authorizer,
            ["affectedUsers"] = auditable.AffectedUsers,
            ["reason"] = auditable.Reason,
        }, Capabilities.Modifications, CancellationToken);

        public Task<IReadOnlyCollection<AuditableCommand?>> GetAsync(ulong guildId, int page = 1) => QueryBuilder
            .Select<AuditableCommand>()
            .Filter(x => x.Guild.Id == guildId)
            .Limit(10)
            .Offset(10 * (page - 1))
            .ExecuteAsync(EdgeDBClient, Capabilities.ReadOnly, CancellationToken);

        public async Task<AuditableCommand?> GetAsync(Ulid auditId) => (await QueryBuilder
            .Select<AuditableCommand>()
            .Filter(x => x.AuditId == auditId)
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
