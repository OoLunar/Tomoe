using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record GuildLoggingModel
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _upsertLogging;
        private static readonly NpgsqlCommand _getLogging;

        public required ulong GuildId { get; init; }
        public required bool Enabled { get; init; }
        public required GuildLoggingType Type { get; init; }
        public required ulong ChannelId { get; init; }
        public required string Format { get; init; }

        static GuildLoggingModel()
        {
            _createTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS guild_logging(
                guild_id BIGINT,
                enabled BOOLEAN NOT NULL,
                type INT NOT NULL,
                channel_id BIGINT NOT NULL,
                format TEXT NOT NULL,
                PRIMARY KEY(guild_id, type)
            );");

            _upsertLogging = new NpgsqlCommand("INSERT INTO guild_logging (guild_id, enabled, type, channel_id, format) VALUES (@guild_id, @enabled, @type, @channel_id, @format) ON CONFLICT (guild_id, type) DO UPDATE SET enabled = @enabled, channel_id = @channel_id, format = @format;");
            _upsertLogging.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _upsertLogging.Parameters.Add(new NpgsqlParameter("@enabled", NpgsqlTypes.NpgsqlDbType.Boolean));
            _upsertLogging.Parameters.Add(new NpgsqlParameter("@type", NpgsqlTypes.NpgsqlDbType.Integer));
            _upsertLogging.Parameters.Add(new NpgsqlParameter("@channel_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _upsertLogging.Parameters.Add(new NpgsqlParameter("@format", NpgsqlTypes.NpgsqlDbType.Text));

            _getLogging = new NpgsqlCommand("SELECT * FROM guild_logging WHERE guild_id = @guild_id AND type = @type;");
            _getLogging.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _getLogging.Parameters.Add(new NpgsqlParameter("@type", NpgsqlTypes.NpgsqlDbType.Integer));
        }

        public static async ValueTask UpsertLoggingAsync(GuildLoggingModel logging)
        {
            await _semaphore.WaitAsync();
            try
            {
                _upsertLogging.Parameters["@guild_id"].Value = (long)logging.GuildId;
                _upsertLogging.Parameters["@enabled"].Value = logging.Enabled;
                _upsertLogging.Parameters["@type"].Value = (int)logging.Type;
                _upsertLogging.Parameters["@channel_id"].Value = (long)logging.ChannelId;
                _upsertLogging.Parameters["@format"].Value = logging.Format;

                await _upsertLogging.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<GuildLoggingModel?> GetLoggingAsync(ulong guildId, GuildLoggingType type)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getLogging.Parameters["@guild_id"].Value = (long)guildId;
                _getLogging.Parameters["@type"].Value = (int)type;

                await using NpgsqlDataReader reader = await _getLogging.ExecuteReaderAsync();
                return !await reader.ReadAsync() ? null : new GuildLoggingModel
                {
                    GuildId = (ulong)reader.GetInt64(0),
                    Enabled = reader.GetBoolean(1),
                    Type = (GuildLoggingType)reader.GetInt32(2),
                    ChannelId = (ulong)reader.GetInt64(3),
                    Format = reader.GetString(4)
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _upsertLogging.Connection = connection;
            _getLogging.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _upsertLogging.PrepareAsync();
            await _getLogging.PrepareAsync();
        }
    }
}
