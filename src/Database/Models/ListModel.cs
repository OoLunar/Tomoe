using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record ListModel
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _createList;
        private static readonly NpgsqlCommand _deleteList;
        private static readonly NpgsqlCommand _getAllLists;
        private static readonly NpgsqlCommand _getListById;
        private static readonly NpgsqlCommand _getListByName;

        public required Ulid Id { get; init; }
        public required ulong UserId { get; init; }
        public required string Name { get; init; }

        static ListModel()
        {
            _createTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS lists(
                id UUID PRIMARY KEY,
                user_id BIGINT,
                name TEXT NOT NULL
            );");

            _createList = new NpgsqlCommand("INSERT INTO lists (id, user_id, name) VALUES (@id, @user_id, @name);");
            _createList.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid));
            _createList.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _createList.Parameters.Add(new NpgsqlParameter("@name", NpgsqlTypes.NpgsqlDbType.Text));

            _deleteList = new NpgsqlCommand("DELETE FROM lists WHERE id = @id;");
            _deleteList.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid));

            _getAllLists = new NpgsqlCommand("SELECT * FROM lists WHERE user_id = @user_id ORDER BY id;");
            _getAllLists.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _getListById = new NpgsqlCommand("SELECT * FROM lists WHERE id = @id;");
            _getListById.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid));

            _getListByName = new NpgsqlCommand("SELECT * FROM lists WHERE name = @name AND user_id = @user_id;");
            _getListByName.Parameters.Add(new NpgsqlParameter("@name", NpgsqlTypes.NpgsqlDbType.Text));
            _getListByName.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
        }

        public static async ValueTask<ListModel> CreateListAsync(ulong userId, string name)
        {
            await _semaphore.WaitAsync();
            try
            {
                Ulid id = Ulid.NewUlid();
                _createList.Parameters["@id"].Value = id;
                _createList.Parameters["@user_id"].Value = (long)userId;
                _createList.Parameters["@name"].Value = name;

                await _createList.ExecuteNonQueryAsync();
                return new ListModel
                {
                    Id = id,
                    UserId = userId,
                    Name = name
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask<bool> DeleteAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                _deleteList.Parameters["@id"].Value = Id;
                return await _deleteList.ExecuteNonQueryAsync() == 1;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async IAsyncEnumerable<ListModel> GetAllListsAsync(ulong userId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getAllLists.Parameters["@user_id"].Value = (long)userId;

                await using NpgsqlDataReader reader = await _getAllLists.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    yield return new ListModel
                    {
                        Id = new Ulid(reader.GetGuid(0)),
                        UserId = (ulong)reader.GetInt64(1),
                        Name = reader.GetString(2)
                    };
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<ListModel?> GetListAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getListById.Parameters["@id"].Value = id;

                await using NpgsqlDataReader reader = await _getListById.ExecuteReaderAsync();
                return !await reader.ReadAsync() ? null : new ListModel
                {
                    Id = new Ulid(reader.GetGuid(0)),
                    UserId = (ulong)reader.GetInt64(1),
                    Name = reader.GetString(2)
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<ListModel?> GetListAsync(string name, ulong userId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getListByName.Parameters["@name"].Value = name;
                _getListByName.Parameters["@user_id"].Value = (long)userId;

                await using NpgsqlDataReader reader = await _getListByName.ExecuteReaderAsync();
                return !await reader.ReadAsync() ? null : new ListModel
                {
                    Id = new Ulid(reader.GetGuid(0)),
                    UserId = (ulong)reader.GetInt64(1),
                    Name = reader.GetString(2)
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
            _createList.Connection = connection;
            _deleteList.Connection = connection;
            _getAllLists.Connection = connection;
            _getListById.Connection = connection;
            _getListByName.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _createList.PrepareAsync();
            await _deleteList.PrepareAsync();
            await _getAllLists.PrepareAsync();
            await _getListById.PrepareAsync();
            await _getListByName.PrepareAsync();
        }
    }
}
