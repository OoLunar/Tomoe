using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record UserSettingsModel
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _getUserCulture;
        private static readonly NpgsqlCommand _getUserTimezone;
        private static readonly NpgsqlCommand _getUserSettings;
        private static readonly NpgsqlCommand _updateUserSettings;

        public required ulong UserId { get; init; }
        public required CultureInfo Culture { get; init; }
        public required TimeZoneInfo Timezone { get; init; }

        static UserSettingsModel()
        {
            _createTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS user_settings(
                user_id BIGINT PRIMARY KEY,
                culture TEXT NOT NULL,
                timezone TEXT NOT NULL
            );");

            _getUserCulture = new NpgsqlCommand("SELECT culture FROM user_settings WHERE user_id = @user_id;");
            _getUserCulture.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _getUserTimezone = new NpgsqlCommand("SELECT timezone FROM user_settings WHERE user_id = @user_id;");
            _getUserTimezone.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _getUserSettings = new NpgsqlCommand("SELECT * FROM user_settings WHERE user_id = @user_id;");
            _getUserSettings.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _updateUserSettings = new NpgsqlCommand("INSERT INTO user_settings (user_id, culture, timezone) VALUES (@user_id, @culture, @timezone) ON CONFLICT (user_id) DO UPDATE SET culture = @culture, timezone = @timezone;");
            _updateUserSettings.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _updateUserSettings.Parameters.Add(new NpgsqlParameter("@culture", NpgsqlTypes.NpgsqlDbType.Text));
            _updateUserSettings.Parameters.Add(new NpgsqlParameter("@timezone", NpgsqlTypes.NpgsqlDbType.Text));
        }

        public static async ValueTask<UserSettingsModel?> GetUserSettingsAsync(ulong userId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getUserSettings.Parameters["@user_id"].Value = (long)userId;

                await using NpgsqlDataReader reader = await _getUserSettings.ExecuteReaderAsync();
                return !await reader.ReadAsync() ? null : new UserSettingsModel
                {
                    UserId = (ulong)reader.GetInt64(0),
                    Culture = CultureInfo.GetCultureInfoByIetfLanguageTag(reader.GetString(1)),
                    Timezone = TimeZoneInfo.FindSystemTimeZoneById(reader.GetString(2))
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<CultureInfo?> GetUserCultureAsync(ulong userId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getUserCulture.Parameters["@user_id"].Value = (long)userId;

                return await _getUserCulture.ExecuteScalarAsync() is not string culture ? null : CultureInfo.GetCultureInfoByIetfLanguageTag(culture);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<TimeZoneInfo?> GetUserTimezoneAsync(ulong userId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getUserTimezone.Parameters["@user_id"].Value = (long)userId;

                return await _getUserTimezone.ExecuteScalarAsync() is not string timezone ? null : TimeZoneInfo.FindSystemTimeZoneById(timezone);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask UpdateUserSettingsAsync(UserSettingsModel settings)
        {
            await _semaphore.WaitAsync();
            try
            {
                _updateUserSettings.Parameters["@user_id"].Value = (long)settings.UserId;
                _updateUserSettings.Parameters["@culture"].Value = settings.Culture.IetfLanguageTag;
                _updateUserSettings.Parameters["@timezone"].Value = settings.Timezone.Id;

                await _updateUserSettings.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _getUserCulture.Connection = connection;
            _getUserTimezone.Connection = connection;
            _getUserSettings.Connection = connection;
            _updateUserSettings.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _getUserCulture.PrepareAsync();
            await _getUserTimezone.PrepareAsync();
            await _getUserSettings.PrepareAsync();
            await _updateUserSettings.PrepareAsync();
        }
    }
}
