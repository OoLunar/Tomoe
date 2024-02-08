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
        private static readonly NpgsqlCommand CountMembers;
        private static readonly NpgsqlCommand GetAllMembers;
        private static readonly NpgsqlCommand BulkUpsert;

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

            CountMembers = new("SELECT COUNT(*) FROM guild_members WHERE guild_id = @guild_id;");
            CountMembers.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            GetAllMembers = new("SELECT * FROM guild_members WHERE guild_id = @guild_id;");
            GetAllMembers.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            BulkUpsert = new(@"INSERT INTO guild_members (user_id, guild_id, first_joined, state, role_ids)
    SELECT user_id, guild_id, first_joined, state, ARRAY[role_id] AS role_ids
    FROM (
        SELECT
            UNNEST(@user_ids) AS user_id,
            UNNEST(@guild_ids) AS guild_id,
            UNNEST(@first_joined) AS first_joined,
            UNNEST(@state) AS state,
            UNNEST(@role_ids) AS role_id
    ) AS temptable
    WHERE user_id IS NOT NULL -- Filter out null values caused by multi-dimensional arrays
    ON CONFLICT (user_id, guild_id) DO UPDATE
    SET
        state = EXCLUDED.state,
        role_ids = EXCLUDED.role_ids;");
            BulkUpsert.Parameters.Add(new("@user_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
            BulkUpsert.Parameters.Add(new("@guild_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
            BulkUpsert.Parameters.Add(new("@first_joined", NpgsqlDbType.Array | NpgsqlDbType.TimestampTz));
            BulkUpsert.Parameters.Add(new("@state", NpgsqlDbType.Array | NpgsqlDbType.Smallint));
            BulkUpsert.Parameters.Add(new("@role_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
        }

        public static async ValueTask<GuildMemberModel> CreateAsync(ulong userId, ulong guildId, DateTimeOffset firstJoined, GuildMemberState state, List<long> roleIds)
        {
            await Semaphore.WaitAsync();
            try
            {
                CreateMember.Parameters["user_id"].Value = (long)userId;
                CreateMember.Parameters["guild_id"].Value = (long)guildId;
                CreateMember.Parameters["first_joined"].Value = firstJoined.UtcDateTime;
                CreateMember.Parameters["state"].Value = (byte)state;
                CreateMember.Parameters["role_ids"].Value = roleIds;

                await CreateMember.ExecuteNonQueryAsync();
                return new GuildMemberModel
                {
                    UserId = userId,
                    GuildId = guildId,
                    FirstJoined = firstJoined,
                    State = state,
                    RoleIds = Unsafe.As<List<long>, List<ulong>>(ref roleIds)
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

        public static async ValueTask<IReadOnlyList<GuildMemberModel>> GetAllMembersAsync(ulong guildId)
        {
            await Semaphore.WaitAsync();
            try
            {
                GetAllMembers.Parameters["guild_id"].Value = (long)guildId;

                await using NpgsqlDataReader reader = await GetAllMembers.ExecuteReaderAsync();
                List<GuildMemberModel> members = [];
                while (await reader.ReadAsync())
                {
                    ulong userId = (ulong)reader.GetInt64(0);
                    DateTimeOffset firstJoined = reader.GetFieldValue<DateTimeOffset>(2);
                    GuildMemberState state = (GuildMemberState)reader.GetFieldValue<byte>(3);
                    long[] roleIds = reader.GetFieldValue<long[]>(4);
                    members.Add(new GuildMemberModel
                    {
                        UserId = userId,
                        GuildId = guildId,
                        FirstJoined = firstJoined,
                        State = state,
                        RoleIds = new(Unsafe.As<long[], ulong[]>(ref roleIds))
                    });
                }

                return members;
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
                BulkUpsert.Parameters["first_joined"].Value = firstJoined.ToArray();
                BulkUpsert.Parameters["state"].Value = states.ToArray();
                BulkUpsert.Parameters["role_ids"].Value = roleIds;

                await BulkUpsert.ExecuteNonQueryAsync();
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask<ulong> CountAllMembersAsync(ulong guildId)
        {
            await Semaphore.WaitAsync();
            try
            {
                CountMembers.Parameters["guild_id"].Value = (long)guildId;
                long count = (long)(await CountMembers.ExecuteScalarAsync())!;
                return Unsafe.As<long, ulong>(ref count);
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

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            CreateTable.Connection = connection;
            CreateMember.Connection = connection;
            FindMember.Connection = connection;
            UpdateMember.Connection = connection;
            DeleteMember.Connection = connection;
            CountMembers.Connection = connection;
            GetAllMembers.Connection = connection;
            BulkUpsert.Connection = connection;

            await CreateTable.ExecuteNonQueryAsync();
            await CreateMember.PrepareAsync();
            await FindMember.PrepareAsync();
            await UpdateMember.PrepareAsync();
            await DeleteMember.PrepareAsync();
            await CountMembers.PrepareAsync();
            await GetAllMembers.PrepareAsync();
            await BulkUpsert.PrepareAsync();
        }
    }
}
