using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record CodeTaskModel
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _create;
        private static readonly NpgsqlCommand _delete;
        private static readonly NpgsqlCommand _getAllGuild;
        private static readonly NpgsqlCommand _getAll;

        public required Ulid Id { get; init; }
        public required ulong GuildId { get; init; }
        public required string Name { get; init; }
        public required string Code { get; init; }

        static CodeTaskModel()
        {
            _createTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS code_task(
                id TEXT NOT NULL PRIMARY KEY,
                guild_id BIGINT,
                name TEXT NOT NULL,
                code TEXT NOT NULL
            );");

            _create = new NpgsqlCommand("INSERT INTO code_task (id, guild_id, name, code) VALUES (@id, @guild_id, @name, @code);");
            _create.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Text));
            _create.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _create.Parameters.Add(new NpgsqlParameter("@name", NpgsqlTypes.NpgsqlDbType.Text));
            _create.Parameters.Add(new NpgsqlParameter("@code", NpgsqlTypes.NpgsqlDbType.Text));

            _delete = new NpgsqlCommand("DELETE FROM code_task WHERE id = @id;");
            _delete.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Text));

            _getAllGuild = new NpgsqlCommand("SELECT * FROM code_task WHERE guild_id = @guild_id;");
            _getAllGuild.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _getAll = new NpgsqlCommand("SELECT * FROM code_task;");
        }

        public static async ValueTask<CodeTaskModel> CreateAsync(Ulid id, ulong guildId, string name, string code)
        {
            await _semaphore.WaitAsync();
            try
            {
                _create.Parameters["@id"].Value = id.ToString();
                _create.Parameters["@guild_id"].Value = (long)guildId;
                _create.Parameters["@name"].Value = name;
                _create.Parameters["@code"].Value = code;
                await _create.ExecuteNonQueryAsync();
                return new CodeTaskModel
                {
                    Id = id,
                    GuildId = guildId,
                    Name = name,
                    Code = code
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<bool> DeleteAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _delete.Parameters["@id"].Value = id.ToString();
                return await _delete.ExecuteNonQueryAsync() is 1;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async IAsyncEnumerable<CodeTaskModel> GetAllGuildAsync(ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getAllGuild.Parameters["@guild_id"].Value = (long)guildId;
                await using NpgsqlDataReader reader = await _getAllGuild.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    yield return new CodeTaskModel
                    {
                        Id = Ulid.Parse(reader.GetString(0), CultureInfo.InvariantCulture),
                        GuildId = (ulong)reader.GetInt64(1),
                        Name = reader.GetString(2),
                        Code = reader.GetString(3)
                    };
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async IAsyncEnumerable<CodeTaskModel> GetAllAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                await using NpgsqlDataReader reader = await _getAll.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    yield return new CodeTaskModel
                    {
                        Id = Ulid.Parse(reader.GetString(0), CultureInfo.InvariantCulture),
                        GuildId = (ulong)reader.GetInt64(1),
                        Name = reader.GetString(2),
                        Code = reader.GetString(3)
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
            _getAllGuild.Connection = connection;
            _getAll.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _create.PrepareAsync();
            await _delete.PrepareAsync();
            await _getAllGuild.PrepareAsync();
            await _getAll.PrepareAsync();
        }

        public static async ValueTask StartAllAsync(NpgsqlConnection connection)
        {
            List<CodeTaskModel> codeTasks = [];
            await foreach (CodeTaskModel codeTask in GetAllAsync())
            {
                codeTasks.Add(codeTask);
            }


        }
    }
}
