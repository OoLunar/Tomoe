using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record GuildSettingsModel
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _getAutoDehoist;
        private static readonly NpgsqlCommand _getAutoDehoistFormat;
        private static readonly NpgsqlCommand _getRestoreRoles;
        private static readonly NpgsqlCommand _getTextPrefix;
        private static readonly NpgsqlCommand _getSettings;
        private static readonly NpgsqlCommand _updateSettings;

        public required ulong GuildId { get; init; }
        public required bool AutoDehoist { get; init; }
        public string? AutoDehoistFormat { get; init; }
        public required bool RestoreRoles { get; init; }
        public string? TextPrefix { get; init; }

        static GuildSettingsModel()
        {
            _createTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS guild_settings(
                guild_id BIGINT PRIMARY KEY,
                auto_dehoist BOOLEAN NOT NULL,
                auto_dehoist_format TEXT,
                restore_roles BOOLEAN NOT NULL,
                text_prefix TEXT
            );");

            _getAutoDehoist = new NpgsqlCommand("SELECT auto_dehoist FROM guild_settings WHERE guild_id = @guild_id;");
            _getAutoDehoist.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _getAutoDehoistFormat = new NpgsqlCommand("SELECT auto_dehoist_format FROM guild_settings WHERE guild_id = @guild_id;");
            _getAutoDehoistFormat.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _getRestoreRoles = new NpgsqlCommand("SELECT restore_roles FROM guild_settings WHERE guild_id = @guild_id;");
            _getRestoreRoles.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _getTextPrefix = new NpgsqlCommand("SELECT text_prefix FROM guild_settings WHERE guild_id = @guild_id;");
            _getTextPrefix.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _getSettings = new NpgsqlCommand("SELECT * FROM guild_settings WHERE guild_id = @guild_id;");
            _getSettings.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _updateSettings = new NpgsqlCommand("INSERT INTO guild_settings (guild_id, auto_dehoist, auto_dehoist_format, restore_roles, text_prefix) VALUES (@guild_id, @auto_dehoist, @auto_dehoist_format, @restore_roles, @text_prefix) ON CONFLICT (guild_id) DO UPDATE SET auto_dehoist = @auto_dehoist, auto_dehoist_format = @auto_dehoist_format, restore_roles = @restore_roles, text_prefix = @text_prefix;");
            _updateSettings.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _updateSettings.Parameters.Add(new NpgsqlParameter("@auto_dehoist", NpgsqlTypes.NpgsqlDbType.Boolean));
            _updateSettings.Parameters.Add(new NpgsqlParameter("@auto_dehoist_format", NpgsqlTypes.NpgsqlDbType.Text));
            _updateSettings.Parameters.Add(new NpgsqlParameter("@restore_roles", NpgsqlTypes.NpgsqlDbType.Boolean));
            _updateSettings.Parameters.Add(new NpgsqlParameter("@text_prefix", NpgsqlTypes.NpgsqlDbType.Text));
        }

        public static async ValueTask<GuildSettingsModel?> GetSettingsAsync(ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getSettings.Parameters["@guild_id"].Value = (long)guildId;

                await using NpgsqlDataReader reader = await _getSettings.ExecuteReaderAsync();
                return reader.Read() ? new GuildSettingsModel()
                {
                    GuildId = (ulong)reader.GetInt64(0),
                    AutoDehoist = reader.GetBoolean(1),
                    AutoDehoistFormat = reader.IsDBNull(2) ? null : reader.GetString(2),
                    RestoreRoles = reader.GetBoolean(3),
                    TextPrefix = reader.IsDBNull(4) ? null : reader.GetString(4)
                } : null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask UpdateSettingsAsync(GuildSettingsModel settings)
        {
            await _semaphore.WaitAsync();
            try
            {
                _updateSettings.Parameters["@guild_id"].Value = (long)settings.GuildId;
                _updateSettings.Parameters["@auto_dehoist"].Value = settings.AutoDehoist;
                _updateSettings.Parameters["@auto_dehoist_format"].Value = settings.AutoDehoistFormat;
                _updateSettings.Parameters["@restore_roles"].Value = settings.RestoreRoles;
                _updateSettings.Parameters["@text_prefix"].Value = settings.TextPrefix;

                await _updateSettings.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<bool> GetAutoDehoistAsync(ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getAutoDehoist.Parameters["@guild_id"].Value = (long)guildId;
                return (bool)(await _getAutoDehoist.ExecuteScalarAsync())!;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<string?> GetAutoDehoistFormatAsync(ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getAutoDehoistFormat.Parameters["@guild_id"].Value = (long)guildId;
                return await _getAutoDehoistFormat.ExecuteScalarAsync() as string;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<bool> GetRestoreRolesAsync(ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getRestoreRoles.Parameters["@guild_id"].Value = (long)guildId;
                return (bool)(await _getRestoreRoles.ExecuteScalarAsync())!;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<string?> GetTextPrefixAsync(ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getTextPrefix.Parameters["@guild_id"].Value = (long)guildId;
                return await _getTextPrefix.ExecuteScalarAsync() as string;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _getAutoDehoist.Connection = connection;
            _getAutoDehoistFormat.Connection = connection;
            _getRestoreRoles.Connection = connection;
            _getTextPrefix.Connection = connection;
            _getSettings.Connection = connection;
            _updateSettings.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _getAutoDehoist.PrepareAsync();
            await _getAutoDehoistFormat.PrepareAsync();
            await _getRestoreRoles.PrepareAsync();
            await _getTextPrefix.PrepareAsync();
            await _getSettings.PrepareAsync();
            await _updateSettings.PrepareAsync();
        }
    }
}
