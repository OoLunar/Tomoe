using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record GuildMemberModel
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _createMember;
        private static readonly NpgsqlCommand _findMember;
        private static readonly NpgsqlCommand _updateMember;
        private static readonly NpgsqlCommand _deleteMember;
        private static readonly NpgsqlCommand _getAllMembers;
        private static readonly NpgsqlCommand _getMembersWithRole;
        private static readonly NpgsqlCommand _bulkUpsert;
        private static readonly NpgsqlCommand _countGuilds;
        private static readonly NpgsqlCommand _countMembers;
        private static readonly NpgsqlCommand _countMembersWithRole;
        private static readonly NpgsqlCommand _countMembersOfGuild;
        private static readonly NpgsqlCommand _findMutualGuild;
        private static readonly NpgsqlCommand _isUserBanned;
        private static readonly NpgsqlCommand _isUserAbsent;

        public required ulong UserId { get; init; }
        public required ulong GuildId { get; init; }
        public required DateTimeOffset FirstJoined { get; init; }
        public required GuildMemberState State { get; set; }
        public required List<ulong> RoleIds { get; set; }

        static GuildMemberModel()
        {
            _createTable = new(@"CREATE TABLE IF NOT EXISTS guild_members(
                user_id bigint NOT NULL,
                guild_id bigint NOT NULL,
                first_joined timestamp with time zone NOT NULL,
                state smallint NOT NULL,
                role_ids bigint[] NOT NULL,
                PRIMARY KEY (user_id, guild_id));
            ");

            _createMember = new("INSERT INTO guild_members (user_id, guild_id, first_joined, state, role_ids) VALUES (@user_id, @guild_id, @first_joined, @state, @role_ids);");
            _createMember.Parameters.Add(new("@user_id", NpgsqlDbType.Bigint));
            _createMember.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));
            _createMember.Parameters.Add(new("@first_joined", NpgsqlDbType.TimestampTz));
            _createMember.Parameters.Add(new("@state", NpgsqlDbType.Smallint));
            _createMember.Parameters.Add(new("@role_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));

            _findMember = new("SELECT * FROM guild_members WHERE user_id = @user_id AND guild_id = @guild_id;");
            _findMember.Parameters.Add(new("@user_id", NpgsqlDbType.Bigint));
            _findMember.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            _updateMember = new("UPDATE guild_members SET first_joined = @first_joined, state = @state, role_ids = @role_ids WHERE user_id = @user_id AND guild_id = @guild_id;");
            _updateMember.Parameters.Add(new("@user_id", NpgsqlDbType.Bigint));
            _updateMember.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));
            _updateMember.Parameters.Add(new("@first_joined", NpgsqlDbType.TimestampTz));
            _updateMember.Parameters.Add(new("@state", NpgsqlDbType.Smallint));
            _updateMember.Parameters.Add(new("@role_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));

            _deleteMember = new("DELETE FROM guild_members WHERE user_id = @user_id AND guild_id = @guild_id;");
            _deleteMember.Parameters.Add(new("@user_id", NpgsqlDbType.Bigint));
            _deleteMember.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            _getAllMembers = new("SELECT * FROM guild_members WHERE guild_id = @guild_id;");
            _getAllMembers.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            _getMembersWithRole = new("SELECT * FROM guild_members WHERE guild_id = @guild_id AND @role_id = ANY(role_ids);");
            _getMembersWithRole.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));
            _getMembersWithRole.Parameters.Add(new("@role_id", NpgsqlDbType.Bigint));

            _bulkUpsert = new(@"INSERT INTO guild_members (user_id, guild_id, first_joined, state, role_ids)
    SELECT user_id, guild_id, first_joined, state, role_ids
        FROM (
            SELECT
                 UNNEST(@user_ids) AS user_id,
                 UNNEST(@guild_ids) AS guild_id,
                 UNNEST(@first_joined_dates) first_joined,
                 UNNEST(@states) AS state,
                 -- https://stackoverflow.com/a/37686469 Turn `[]` (jsonb) into `{}` postgres array
                 TRANSLATE(jsonb_array_elements(@role_ids)::text, '[]', '{}')::bigint[] AS role_ids
            ) AS _
    ON CONFLICT (user_id, guild_id) DO UPDATE
    SET
        state = EXCLUDED.state,
        role_ids = EXCLUDED.role_ids;");
            _bulkUpsert.Parameters.Add(new("@user_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
            _bulkUpsert.Parameters.Add(new("@guild_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
            _bulkUpsert.Parameters.Add(new("@first_joined_dates", NpgsqlDbType.Array | NpgsqlDbType.TimestampTz));
            _bulkUpsert.Parameters.Add(new("@states", NpgsqlDbType.Array | NpgsqlDbType.Smallint));
            _bulkUpsert.Parameters.Add(new("@role_ids", NpgsqlDbType.Jsonb));

            _countGuilds = new("SELECT COUNT(DISTINCT guild_id) FROM guild_members;");
            _countMembers = new("SELECT COUNT(user_id) FROM guild_members;");

            _countMembersWithRole = new("SELECT COUNT(user_id) FROM guild_members WHERE guild_id = @guild_id AND @role_id = ANY(role_ids);");
            _countMembersWithRole.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));
            _countMembersWithRole.Parameters.Add(new("@role_id", NpgsqlDbType.Bigint));

            _countMembersOfGuild = new("SELECT COUNT(guild_id) FROM guild_members WHERE guild_id = @guild_id AND state & 1 = 0;");
            _countMembersOfGuild.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            _findMutualGuild = new("SELECT guild_id FROM guild_members WHERE user_id = @user_id AND state | 1 = 1;");
            _findMutualGuild.Parameters.Add(new("@user_id", NpgsqlDbType.Bigint));

            _isUserBanned = new("SELECT EXISTS(SELECT 1 FROM guild_members WHERE user_id = @user_id AND guild_id = @guild_id AND state & 3 = 1);");
            _isUserBanned.Parameters.Add(new("@user_id", NpgsqlDbType.Bigint));
            _isUserBanned.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            _isUserAbsent = new("SELECT EXISTS(SELECT 1 FROM guild_members WHERE user_id = @user_id AND guild_id = @guild_id AND state & 1 = 0);");
            _isUserAbsent.Parameters.Add(new("@user_id", NpgsqlDbType.Bigint));
            _isUserAbsent.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));
        }

        public static async ValueTask<GuildMemberModel> CreateAsync(ulong userId, ulong guildId, DateTimeOffset firstJoined, GuildMemberState state, IEnumerable<ulong> roleIds)
        {
            await _semaphore.WaitAsync();
            try
            {
                _createMember.Parameters["user_id"].Value = (long)userId;
                _createMember.Parameters["guild_id"].Value = (long)guildId;
                _createMember.Parameters["first_joined"].Value = firstJoined.UtcDateTime;
                _createMember.Parameters["state"].Value = (byte)state;
                _createMember.Parameters["role_ids"].Value = new List<long>(roleIds.Select(value => (long)value));

                await _createMember.ExecuteNonQueryAsync();
                return new GuildMemberModel
                {
                    UserId = userId,
                    GuildId = guildId,
                    FirstJoined = firstJoined,
                    State = state,
                    RoleIds = roleIds.ToList()
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<GuildMemberModel?> FindMemberAsync(ulong userId, ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _findMember.Parameters["user_id"].Value = (long)userId;
                _findMember.Parameters["guild_id"].Value = (long)guildId;

                await using NpgsqlDataReader reader = await _findMember.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }

                DateTimeOffset firstJoined = reader.GetFieldValue<DateTimeOffset>(2);
                GuildMemberState state = (GuildMemberState)reader.GetFieldValue<byte>(3);
                long[] roleIds = reader.GetFieldValue<long[]>(4);
                return new GuildMemberModel
                {
                    UserId = userId,
                    GuildId = guildId,
                    FirstJoined = firstJoined,
                    State = state,
                    RoleIds = new(Unsafe.As<long[], ulong[]>(ref roleIds))
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async IAsyncEnumerable<GuildMemberModel> GetAllMembersAsync(ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getAllMembers.Parameters["guild_id"].Value = (long)guildId;

                await using NpgsqlDataReader reader = await _getAllMembers.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ulong userId = (ulong)reader.GetInt64(0);
                    DateTimeOffset firstJoined = reader.GetFieldValue<DateTimeOffset>(2);
                    GuildMemberState state = (GuildMemberState)reader.GetFieldValue<byte>(3);
                    long[] roleIds = reader.GetFieldValue<long[]>(4);
                    yield return new GuildMemberModel
                    {
                        UserId = userId,
                        GuildId = guildId,
                        FirstJoined = firstJoined,
                        State = state,
                        RoleIds = new(Unsafe.As<long[], ulong[]>(ref roleIds))
                    };
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<ulong> CountMembersWithRoleAsync(ulong guildId, ulong roleId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _countMembersWithRole.Parameters["guild_id"].Value = (long)guildId;
                _countMembersWithRole.Parameters["role_id"].Value = (long)roleId;

                using NpgsqlDataReader reader = await _countMembersWithRole.ExecuteReaderAsync();
                await reader.ReadAsync();
                long count = reader.GetInt64(0)!;
                return Unsafe.As<long, ulong>(ref count);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async IAsyncEnumerable<GuildMemberModel> GetMembersWithRoleAsync(ulong guildId, ulong roleId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getMembersWithRole.Parameters["guild_id"].Value = (long)guildId;
                _getMembersWithRole.Parameters["role_id"].Value = (long)roleId;

                await using NpgsqlDataReader reader = await _getMembersWithRole.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ulong userId = (ulong)reader.GetInt64(0);
                    DateTimeOffset firstJoined = reader.GetFieldValue<DateTimeOffset>(2);
                    GuildMemberState state = (GuildMemberState)reader.GetFieldValue<byte>(3);
                    long[] roleIds = reader.GetFieldValue<long[]>(4);
                    yield return new GuildMemberModel
                    {
                        UserId = userId,
                        GuildId = guildId,
                        FirstJoined = firstJoined,
                        State = state,
                        RoleIds = new(Unsafe.As<long[], ulong[]>(ref roleIds))
                    };
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask BulkUpsertAsync(IEnumerable<GuildMemberModel> members)
        {
            await _semaphore.WaitAsync();
            try
            {
                List<long> userIds = [];
                List<long> guildIds = [];
                List<DateTimeOffset> firstJoined = [];
                List<byte> states = [];
                List<List<long>> roleIds = [];
                foreach (GuildMemberModel member in members)
                {
                    userIds.Add((long)member.UserId);
                    guildIds.Add((long)member.GuildId);
                    firstJoined.Add(member.FirstJoined.UtcDateTime);
                    states.Add((byte)member.State);
                    roleIds.Add(new List<long>(member.RoleIds.Select(value => (long)value)));
                }

                _bulkUpsert.Parameters["user_ids"].Value = userIds.ToArray();
                _bulkUpsert.Parameters["guild_ids"].Value = guildIds.ToArray();
                _bulkUpsert.Parameters["first_joined_dates"].Value = firstJoined.ToArray();
                _bulkUpsert.Parameters["states"].Value = states.ToArray();
                _bulkUpsert.Parameters["role_ids"].Value = JsonSerializer.Serialize(roleIds);

                await _bulkUpsert.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask UpdateAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                _updateMember.Parameters["user_id"].Value = (long)UserId;
                _updateMember.Parameters["guild_id"].Value = (long)GuildId;
                _updateMember.Parameters["first_joined"].Value = FirstJoined;
                _updateMember.Parameters["state"].Value = (byte)State;

                long[] roleIdsArray = [.. RoleIds.Select(value => (long)value).ToArray()];
                _updateMember.Parameters["role_ids"].Value = roleIdsArray;

                await _updateMember.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask DeleteAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                _deleteMember.Parameters["user_id"].Value = (long)UserId;
                _deleteMember.Parameters["guild_id"].Value = (long)GuildId;

                await _deleteMember.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<ulong> CountGuildsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                long count = (long)(await _countGuilds.ExecuteScalarAsync())!;
                return Unsafe.As<long, ulong>(ref count);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<ulong> CountMembersAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                long count = (long)(await _countMembers.ExecuteScalarAsync())!;
                return Unsafe.As<long, ulong>(ref count);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<ulong> CountMembersAsync(ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _countMembersOfGuild.Parameters["guild_id"].Value = (long)guildId;
                long count = (long)(await _countMembersOfGuild.ExecuteScalarAsync())!;
                return Unsafe.As<long, ulong>(ref count);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<IReadOnlyList<ulong>> FindMutualGuildsAsync(ulong userId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _findMutualGuild.Parameters["user_id"].Value = (long)userId;

                await using NpgsqlDataReader reader = await _findMutualGuild.ExecuteReaderAsync();
                List<ulong> guildIds = [];
                while (await reader.ReadAsync())
                {
                    guildIds.Add((ulong)reader.GetInt64(0));
                }

                return guildIds;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<bool> IsUserBannedAsync(ulong userId, ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _isUserBanned.Parameters["user_id"].Value = (long)userId;
                _isUserBanned.Parameters["guild_id"].Value = (long)guildId;

                return (bool)(await _isUserBanned.ExecuteScalarAsync())!;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<bool> IsUserAbsentAsync(ulong userId, ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _isUserAbsent.Parameters["user_id"].Value = (long)userId;
                _isUserAbsent.Parameters["guild_id"].Value = (long)guildId;

                return !(bool)(await _isUserAbsent.ExecuteScalarAsync())!;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _createMember.Connection = connection;
            _findMember.Connection = connection;
            _updateMember.Connection = connection;
            _deleteMember.Connection = connection;
            _getAllMembers.Connection = connection;
            _getMembersWithRole.Connection = connection;
            _bulkUpsert.Connection = connection;
            _countGuilds.Connection = connection;
            _countMembers.Connection = connection;
            _countMembersWithRole.Connection = connection;
            _countMembersOfGuild.Connection = connection;
            _findMutualGuild.Connection = connection;
            _isUserBanned.Connection = connection;
            _isUserAbsent.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _createMember.PrepareAsync();
            await _findMember.PrepareAsync();
            await _updateMember.PrepareAsync();
            await _deleteMember.PrepareAsync();
            await _getAllMembers.PrepareAsync();
            await _getMembersWithRole.PrepareAsync();
            await _bulkUpsert.PrepareAsync();
            await _countGuilds.PrepareAsync();
            await _countMembers.PrepareAsync();
            await _countMembersWithRole.PrepareAsync();
            await _countMembersOfGuild.PrepareAsync();
            await _findMutualGuild.PrepareAsync();
            await _isUserBanned.PrepareAsync();
            await _isUserAbsent.PrepareAsync();
        }
    }
}
