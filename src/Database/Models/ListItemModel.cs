using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record ListItemModel : IComparable
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _createItem;
        private static readonly NpgsqlCommand _deleteItem;
        private static readonly NpgsqlCommand _getAllItems;
        private static readonly NpgsqlCommand _getItem;
        private static readonly NpgsqlCommand _updateItem;
        private static readonly NpgsqlCommand _countItems;

        public required Ulid Id { get; init; }
        public required Ulid ListId { get; init; }
        public required string Content { get; set; }
        public bool IsChecked { get; set; }

        static ListItemModel()
        {
            _createTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS lists_items(
                id UUID PRIMARY KEY,
                list_id UUID NOT NULL,
                content TEXT NOT NULL,
                is_checked BOOLEAN NOT NULL DEFAULT FALSE,
                FOREIGN KEY (list_id) REFERENCES lists(id) ON DELETE CASCADE
            );");

            _createItem = new NpgsqlCommand(@"INSERT INTO lists_items(id, list_id, content, is_checked) VALUES(@id, @list_id, @content, @is_checked);");
            _createItem.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid));
            _createItem.Parameters.Add(new NpgsqlParameter("@list_id", NpgsqlTypes.NpgsqlDbType.Uuid));
            _createItem.Parameters.Add(new NpgsqlParameter("@content", NpgsqlTypes.NpgsqlDbType.Text));
            _createItem.Parameters.Add(new NpgsqlParameter("@is_checked", NpgsqlTypes.NpgsqlDbType.Boolean));

            _deleteItem = new NpgsqlCommand("DELETE FROM lists_items WHERE id = @id;");
            _deleteItem.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid));

            _getAllItems = new NpgsqlCommand("SELECT id, list_id, content, is_checked FROM lists_items WHERE list_id = @list_id ORDER BY id;");
            _getAllItems.Parameters.Add(new NpgsqlParameter("@list_id", NpgsqlTypes.NpgsqlDbType.Uuid));

            _getItem = new NpgsqlCommand("SELECT id, list_id, content, is_checked FROM lists_items WHERE id = @id;");
            _getItem.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid));

            _updateItem = new NpgsqlCommand("UPDATE lists_items SET content = @content, is_checked = @is_checked WHERE id = @id;");
            _updateItem.Parameters.Add(new NpgsqlParameter("@id", NpgsqlTypes.NpgsqlDbType.Uuid));
            _updateItem.Parameters.Add(new NpgsqlParameter("@content", NpgsqlTypes.NpgsqlDbType.Text));
            _updateItem.Parameters.Add(new NpgsqlParameter("@is_checked", NpgsqlTypes.NpgsqlDbType.Boolean));

            _countItems = new NpgsqlCommand("SELECT COUNT(id) FROM lists_items WHERE list_id = @list_id;");
            _countItems.Parameters.Add(new NpgsqlParameter("@list_id", NpgsqlTypes.NpgsqlDbType.Uuid));
        }

        public static async ValueTask<ListItemModel> CreateAsync(Ulid listId, string content, bool isChecked = false)
        {
            await _semaphore.WaitAsync();
            try
            {
                Ulid id = Ulid.NewUlid();
                _createItem.Parameters["@id"].Value = id.ToGuid();
                _createItem.Parameters["@list_id"].Value = listId;
                _createItem.Parameters["@content"].Value = content;
                _createItem.Parameters["@is_checked"].Value = isChecked;

                await _createItem.ExecuteNonQueryAsync();
                return new ListItemModel
                {
                    Id = id,
                    ListId = listId,
                    Content = content,
                    IsChecked = isChecked
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
                _deleteItem.Parameters["@id"].Value = Id.ToGuid();
                return await _deleteItem.ExecuteNonQueryAsync() == 1;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async IAsyncEnumerable<ListItemModel> GetAllAsync(Ulid listId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getAllItems.Parameters["@list_id"].Value = listId.ToGuid();
                await using NpgsqlDataReader reader = await _getAllItems.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    yield return new ListItemModel
                    {
                        Id = new Ulid(reader.GetGuid(0)),
                        ListId = new Ulid(reader.GetGuid(1)),
                        Content = reader.GetString(2),
                        IsChecked = reader.GetBoolean(3)
                    };
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<ListItemModel?> GetAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getItem.Parameters["@id"].Value = id.ToGuid();
                await using NpgsqlDataReader reader = await _getItem.ExecuteReaderAsync();
                return !await reader.ReadAsync() ? null : new ListItemModel
                {
                    Id = new Ulid(reader.GetGuid(0)),
                    ListId = new Ulid(reader.GetGuid(1)),
                    Content = reader.GetString(2),
                    IsChecked = reader.GetBoolean(3)
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask<bool> UpdateAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                _updateItem.Parameters["@id"].Value = Id.ToGuid();
                _updateItem.Parameters["@content"].Value = Content;
                _updateItem.Parameters["@is_checked"].Value = IsChecked;

                return await _updateItem.ExecuteNonQueryAsync() == 1;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<int> CountAsync(Ulid listId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _countItems.Parameters["@list_id"].Value = listId.ToGuid();
                return (int)(await _countItems.ExecuteScalarAsync())!;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _createItem.Connection = connection;
            _deleteItem.Connection = connection;
            _getAllItems.Connection = connection;
            _getItem.Connection = connection;
            _updateItem.Connection = connection;
            _countItems.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _createItem.PrepareAsync();
            await _deleteItem.PrepareAsync();
            await _getAllItems.PrepareAsync();
            await _getItem.PrepareAsync();
            await _updateItem.PrepareAsync();
            await _countItems.PrepareAsync();
        }

        public int CompareTo(object? obj) => Id.CompareTo(obj);
    }
}
