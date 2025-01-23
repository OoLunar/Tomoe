using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record AutoPublishModel
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _create;
        private static readonly NpgsqlCommand _delete;
        private static readonly NpgsqlCommand _exists;
        private static readonly NpgsqlCommand _getAllGuild;

        public required ulong GuildId { get; init; }
        public required ulong ChannelId { get; init; }

        static AutoPublishModel()
        {
            _createTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS auto_publish(
                guild_id BIGINT,
                channel_id BIGINT,
                PRIMARY KEY(guild_id)
            );");

            _create = new NpgsqlCommand("INSERT INTO auto_publish (channel_id, guild_id) VALUES (@channel_id, @guild_id);");
            _create.Parameters.Add(new NpgsqlParameter("@channel_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _create.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _delete = new NpgsqlCommand("DELETE FROM auto_publish WHERE channel_id = @channel_id AND guild_id = @guild_id;");
            _delete.Parameters.Add(new NpgsqlParameter("@channel_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _delete.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _exists = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM auto_publish WHERE channel_id = @channel_id AND guild_id = @guild_id);");
            _exists.Parameters.Add(new NpgsqlParameter("@channel_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _exists.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _getAllGuild = new NpgsqlCommand("SELECT * FROM auto_publish WHERE guild_id = @guild_id;");
            _getAllGuild.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));
        }

        public static async ValueTask CreateTableAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                await _createTable.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask CreateAsync(ulong guildId, ulong channelId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _create.Parameters["@guild_id"].Value = (long)guildId;
                _create.Parameters["@channel_id"].Value = (long)channelId;

                await _create.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask DeleteAsync(ulong guildId, ulong channelId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _delete.Parameters["@channel_id"].Value = (long)channelId;
                _delete.Parameters["@guild_id"].Value = (long)guildId;

                await _delete.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<bool> ExistsAsync(ulong guildId, ulong channelId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _exists.Parameters["@channel_id"].Value = (long)channelId;
                _exists.Parameters["@guild_id"].Value = (long)guildId;

                object? result = await _exists.ExecuteScalarAsync();
                return result is not null and not false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async IAsyncEnumerable<AutoPublishModel> GetAllGuildAsync(ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getAllGuild.Parameters["@guild_id"].Value = (long)guildId;

                await using NpgsqlDataReader reader = await _getAllGuild.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    yield return new AutoPublishModel
                    {
                        ChannelId = (ulong)reader.GetInt64(0),
                        GuildId = (ulong)reader.GetInt64(1),
                    };
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _create.Connection = connection;
            _delete.Connection = connection;
            _exists.Connection = connection;
            _getAllGuild.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _create.PrepareAsync();
            await _delete.PrepareAsync();
            await _exists.PrepareAsync();
            await _getAllGuild.PrepareAsync();
        }
    }
}
