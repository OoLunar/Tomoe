using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record GuildMemberModel
    {
        private static readonly SemaphoreSlim Semaphore = new(1, 1);
        private static readonly NpgsqlCommand CreateTable;
        private static readonly NpgsqlCommand CreateMember;
        private static readonly NpgsqlCommand FindMember;
        private static readonly NpgsqlCommand UpdateMember;
        private static readonly NpgsqlCommand DeleteMember;
        private static readonly NpgsqlCommand GetAllMembers;
        private static readonly NpgsqlCommand GetMembersWithRole;
        private static readonly NpgsqlCommand BulkUpsert;
        private static readonly NpgsqlCommand CountGuilds;
        private static readonly NpgsqlCommand CountMembers;
        private static readonly NpgsqlCommand CountMembersOfGuild;

        public ulong UserId { get; init; }
        public ulong GuildId { get; init; }
        public DateTimeOffset FirstJoined { get; init; }
        public GuildMemberState State { get; set; }
        public List<ulong> RoleIds { get; set; } = [];

        static GuildMemberModel()
        {
            CreateTable = new(@"CREATE TABLE IF NOT EXISTS guild_members(
                user_id bigint NOT NULL,
                guild_id bigint NOT NULL,
                first_joined timestamp with time zone NOT NULL,
                state smallint NOT NULL,
                role_ids bigint[] NOT NULL,
                PRIMARY KEY (user_id, guild_id));
            ");

            CreateMember = new("INSERT INTO guild_members (user_id, guild_id, first_joined, state, role_ids) VALUES (@user_id, @guild_id, @first_joined, @state, @role_ids);");
            CreateMember.Parameters.Add(new("@user_id", NpgsqlDbType.Bigint));
            CreateMember.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));
            CreateMember.Parameters.Add(new("@first_joined", NpgsqlDbType.TimestampTz));
            CreateMember.Parameters.Add(new("@state", NpgsqlDbType.Smallint));
            CreateMember.Parameters.Add(new("@role_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));

            FindMember = new("SELECT * FROM guild_members WHERE user_id = @user_id AND guild_id = @guild_id;");
            FindMember.Parameters.Add(new("@user_id", NpgsqlDbType.Bigint));
            FindMember.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            UpdateMember = new("UPDATE guild_members SET first_joined = @first_joined, state = @state, role_ids = @role_ids WHERE user_id = @user_id AND guild_id = @guild_id;");
            UpdateMember.Parameters.Add(new("@user_id", NpgsqlDbType.Bigint));
            UpdateMember.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));
            UpdateMember.Parameters.Add(new("@first_joined", NpgsqlDbType.TimestampTz));
            UpdateMember.Parameters.Add(new("@state", NpgsqlDbType.Smallint));
            UpdateMember.Parameters.Add(new("@role_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));

            DeleteMember = new("DELETE FROM guild_members WHERE user_id = @user_id AND guild_id = @guild_id;");
            DeleteMember.Parameters.Add(new("@user_id", NpgsqlDbType.Bigint));
            DeleteMember.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            CountMembersOfGuild = new("SELECT COUNT(*) FROM guild_members WHERE guild_id = @guild_id;");
            CountMembersOfGuild.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            GetAllMembers = new("SELECT * FROM guild_members WHERE guild_id = @guild_id;");
            GetAllMembers.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            GetMembersWithRole = new("SELECT * FROM guild_members WHERE guild_id = @guild_id AND @role_id = ANY(role_ids);");
            GetMembersWithRole.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));
            GetMembersWithRole.Parameters.Add(new("@role_id", NpgsqlDbType.Bigint));

            BulkUpsert = new(@"INSERT INTO guild_members (user_id, guild_id, first_joined, state, role_ids)
    SELECT user_id, guild_id, first_joined, state, idx
        FROM (
            SELECT
                 UNNEST(@user_ids) AS user_id,
                 UNNEST(@guild_ids) AS guild_id,
                 UNNEST(@first_joined_dates) first_joined,
                 UNNEST(@states) AS state,
                 -- https://stackoverflow.com/a/61679054 (ab)use`jsonb` to de-nest the arrays
                 -- https://stackoverflow.com/a/37686469 Turn `[]` (jsonb) into `{}` postgres array
                 TRANSLATE(jsonb_array_elements(to_jsonb(@role_ids::bigint[][]))::text, '[]', '{}')::bigint[] AS idx
         ) AS _
    WHERE user_id IS NOT NULL -- Filter out null values caused by multi-dimensional arrays
    ON CONFLICT (user_id, guild_id) DO UPDATE
    SET
        state = EXCLUDED.state,
        role_ids = EXCLUDED.role_ids;");
            BulkUpsert.Parameters.Add(new("@user_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
            BulkUpsert.Parameters.Add(new("@guild_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
            BulkUpsert.Parameters.Add(new("@first_joined_dates", NpgsqlDbType.Array | NpgsqlDbType.TimestampTz));
            BulkUpsert.Parameters.Add(new("@states", NpgsqlDbType.Array | NpgsqlDbType.Smallint));
            BulkUpsert.Parameters.Add(new("@role_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));

            CountGuilds = new("SELECT COUNT(DISTINCT guild_id) FROM guild_members;");
            CountMembers = new("SELECT COUNT(*) FROM guild_members;");
        }

        public static async ValueTask<GuildMemberModel> CreateAsync(ulong userId, ulong guildId, DateTimeOffset firstJoined, GuildMemberState state, IEnumerable<ulong> roleIds)
        {
            await Semaphore.WaitAsync();
            try
            {
                CreateMember.Parameters["user_id"].Value = (long)userId;
                CreateMember.Parameters["guild_id"].Value = (long)guildId;
                CreateMember.Parameters["first_joined"].Value = firstJoined.UtcDateTime;
                CreateMember.Parameters["state"].Value = (byte)state;
                CreateMember.Parameters["role_ids"].Value = new List<long>(Unsafe.As<IEnumerable<ulong>, IEnumerable<long>>(ref roleIds));

                await CreateMember.ExecuteNonQueryAsync();
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
                Semaphore.Release();
            }
        }

        public static async ValueTask<GuildMemberModel?> FindMemberAsync(ulong userId, ulong guildId)
        {
            await Semaphore.WaitAsync();
            try
            {
                FindMember.Parameters["user_id"].Value = (long)userId;
                FindMember.Parameters["guild_id"].Value = (long)guildId;

                await using NpgsqlDataReader reader = await FindMember.ExecuteReaderAsync();
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
                Semaphore.Release();
            }
        }

        public static async IAsyncEnumerable<GuildMemberModel> GetAllMembersAsync(ulong guildId)
        {
            await Semaphore.WaitAsync();
            try
            {
                GetAllMembers.Parameters["guild_id"].Value = (long)guildId;

                await using NpgsqlDataReader reader = await GetAllMembers.ExecuteReaderAsync();
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
                Semaphore.Release();
            }
        }

        public static async IAsyncEnumerable<GuildMemberModel> GetMembersWithRoleAsync(ulong guildId, ulong roleId)
        {
            await Semaphore.WaitAsync();
            try
            {
                GetMembersWithRole.Parameters["guild_id"].Value = (long)guildId;
                GetMembersWithRole.Parameters["role_id"].Value = (long)roleId;

                await using NpgsqlDataReader reader = await GetMembersWithRole.ExecuteReaderAsync();
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
                Semaphore.Release();
            }
        }

        public static async ValueTask BulkUpsertAsync(IEnumerable<GuildMemberModel> members)
        {
            await Semaphore.WaitAsync();
            try
            {
                List<long> userIds = [];
                List<long> guildIds = [];
                List<DateTimeOffset> firstJoined = [];
                List<byte> states = [];
                long[,] roleIds = new long[members.Count(), members.Max(member => member.RoleIds.Count)];
                foreach (GuildMemberModel member in members)
                {
                    userIds.Add((long)member.UserId);
                    guildIds.Add((long)member.GuildId);
                    firstJoined.Add(member.FirstJoined.UtcDateTime);
                    states.Add((byte)member.State);
                    for (int i = 0; i < member.RoleIds.Count; i++)
                    {
                        roleIds[userIds.Count - 1, i] = (long)member.RoleIds[i];
                    }
                }

                BulkUpsert.Parameters["user_ids"].Value = userIds.ToArray();
                BulkUpsert.Parameters["guild_ids"].Value = guildIds.ToArray();
                BulkUpsert.Parameters["first_joined_dates"].Value = firstJoined.ToArray();
                BulkUpsert.Parameters["states"].Value = states.ToArray();
                BulkUpsert.Parameters["role_ids"].Value = roleIds;

                await BulkUpsert.ExecuteNonQueryAsync();
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async ValueTask UpdateAsync()
        {
            await Semaphore.WaitAsync();
            try
            {
                UpdateMember.Parameters["user_id"].Value = (long)UserId;
                UpdateMember.Parameters["guild_id"].Value = (long)GuildId;
                UpdateMember.Parameters["first_joined"].Value = FirstJoined;
                UpdateMember.Parameters["state"].Value = (byte)State;

                long[] roleIdsArray = [.. RoleIds.Select(value => (long)value).ToArray()];
                UpdateMember.Parameters["role_ids"].Value = roleIdsArray;

                await UpdateMember.ExecuteNonQueryAsync();
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public async ValueTask DeleteAsync()
        {
            await Semaphore.WaitAsync();
            try
            {
                DeleteMember.Parameters["user_id"].Value = (long)UserId;
                DeleteMember.Parameters["guild_id"].Value = (long)GuildId;

                await DeleteMember.ExecuteNonQueryAsync();
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask<ulong> CountGuildsAsync()
        {
            await Semaphore.WaitAsync();
            try
            {
                long count = (long)(await CountGuilds.ExecuteScalarAsync())!;
                return Unsafe.As<long, ulong>(ref count);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask<ulong> CountMembersAsync()
        {
            await Semaphore.WaitAsync();
            try
            {
                long count = (long)(await CountMembers.ExecuteScalarAsync())!;
                return Unsafe.As<long, ulong>(ref count);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask<ulong> CountMembersAsync(ulong guildId)
        {
            await Semaphore.WaitAsync();
            try
            {
                CountMembersOfGuild.Parameters["guild_id"].Value = (long)guildId;
                long count = (long)(await CountMembersOfGuild.ExecuteScalarAsync())!;
                return Unsafe.As<long, ulong>(ref count);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            CreateTable.Connection = connection;
            CreateMember.Connection = connection;
            FindMember.Connection = connection;
            UpdateMember.Connection = connection;
            DeleteMember.Connection = connection;
            GetAllMembers.Connection = connection;
            GetMembersWithRole.Connection = connection;
            BulkUpsert.Connection = connection;
            CountGuilds.Connection = connection;
            CountMembers.Connection = connection;
            CountMembersOfGuild.Connection = connection;

            await CreateTable.ExecuteNonQueryAsync();
            await CreateMember.PrepareAsync();
            await FindMember.PrepareAsync();
            await UpdateMember.PrepareAsync();
            await DeleteMember.PrepareAsync();
            await GetAllMembers.PrepareAsync();
            await GetMembersWithRole.PrepareAsync();
            await BulkUpsert.PrepareAsync();
            await CountGuilds.PrepareAsync();
            await CountMembers.PrepareAsync();
            await CountMembersOfGuild.PrepareAsync();
        }
    }
}