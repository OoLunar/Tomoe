using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record TagHistoryModel
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _newRevision;
        private static readonly NpgsqlCommand _getRevisions;
        private static readonly NpgsqlCommand _getRevision;
        private static readonly NpgsqlCommand _deleteHistory;
        private static readonly NpgsqlCommand _countRevisions;

        static TagHistoryModel()
        {
            _createTable = new(@"CREATE TABLE IF NOT EXISTS tag_history(
                id UUID,
                name TEXT NOT NULL,
                content TEXT NOT NULL,
                owner_id BIGINT NOT NULL,
                guild_id BIGINT NOT NULL,
                last_updated_at TIMESTAMPTZ NOT NULL,
                uses BIGINT NOT NULL
            );");

            _newRevision = new(@"INSERT INTO tag_history (id, name, content, owner_id, guild_id, last_updated_at, uses) VALUES (@id, @name, @content, @owner_id, @guild_id, @last_updated_at, @uses);");
            _newRevision.Parameters.Add(new("@id", NpgsqlDbType.Uuid));
            _newRevision.Parameters.Add(new("@name", NpgsqlDbType.Text));
            _newRevision.Parameters.Add(new("@content", NpgsqlDbType.Text));
            _newRevision.Parameters.Add(new("@owner_id", NpgsqlDbType.Bigint));
            _newRevision.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));
            _newRevision.Parameters.Add(new("@last_updated_at", NpgsqlDbType.TimestampTz));
            _newRevision.Parameters.Add(new("@uses", NpgsqlDbType.Bigint));

            _getRevisions = new(@"SELECT * FROM tag_history WHERE id = @id ORDER BY last_updated_at DESC;");
            _getRevisions.Parameters.Add(new("@id", NpgsqlDbType.Uuid));

            // Select row at X index to get a specific revision.
            _getRevision = new(@"SELECT * FROM tag_history WHERE id = @id ORDER BY last_updated_at DESC LIMIT 1 OFFSET @index;");
            _getRevision.Parameters.Add(new("@id", NpgsqlDbType.Uuid));
            _getRevision.Parameters.Add(new("@index", NpgsqlDbType.Integer));

            _deleteHistory = new(@"DELETE FROM tag_history WHERE id = @id;");
            _deleteHistory.Parameters.Add(new("@id", NpgsqlDbType.Uuid));

            _countRevisions = new(@"SELECT COUNT(*) FROM tag_history WHERE id = @id;");
            _countRevisions.Parameters.Add(new("@id", NpgsqlDbType.Uuid));
        }

        public static async ValueTask CreateRevisionAsync(TagModel tag)
        {
            await _semaphore.WaitAsync();
            try
            {
                _newRevision.Parameters["@id"].Value = tag.Id.ToGuid();
                _newRevision.Parameters["@name"].Value = tag.Name;
                _newRevision.Parameters["@content"].Value = tag.Content;
                _newRevision.Parameters["@owner_id"].Value = (long)tag.OwnerId;
                _newRevision.Parameters["@guild_id"].Value = (long)tag.GuildId;
                _newRevision.Parameters["@last_updated_at"].Value = DateTime.UtcNow;
                _newRevision.Parameters["@uses"].Value = (long)tag.Uses;

                await _newRevision.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<IReadOnlyList<TagModel>> GetRevisionsAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getRevisions.Parameters["@id"].Value = id.ToGuid();

                await using NpgsqlDataReader reader = await _getRevisions.ExecuteReaderAsync();
                List<TagModel> revisions = [];
                while (await reader.ReadAsync())
                {
                    revisions.Add(new TagModel
                    {
                        Id = new Ulid(reader.GetGuid(0)),
                        Name = reader.GetString(1),
                        Content = reader.GetString(2),
                        OwnerId = (ulong)reader.GetInt64(3),
                        GuildId = (ulong)reader.GetInt64(4),
                        LastUpdatedAt = reader.GetDateTime(5),
                        Uses = (ulong)reader.GetInt64(6)
                    });
                }

                return revisions;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<TagModel?> GetRevisionAsync(Ulid id, int index)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getRevision.Parameters["@id"].Value = id.ToGuid();
                _getRevision.Parameters["@index"].Value = index;

                await using NpgsqlDataReader reader = await _getRevision.ExecuteReaderAsync();
                return !await reader.ReadAsync() ? null : new TagModel
                {
                    Id = new Ulid(reader.GetGuid(0)),
                    Name = reader.GetString(1),
                    Content = reader.GetString(2),
                    OwnerId = (ulong)reader.GetInt64(3),
                    GuildId = (ulong)reader.GetInt64(4),
                    LastUpdatedAt = reader.GetDateTime(5),
                    Uses = (ulong)reader.GetInt64(6)
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask DeleteHistoryAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _deleteHistory.Parameters["@id"].Value = id.ToGuid();
                await _deleteHistory.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<ulong> CountRevisionsAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _countRevisions.Parameters["@id"].Value = id.ToGuid();
                return ulong.CreateChecked((long)(await _countRevisions.ExecuteScalarAsync())!);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _newRevision.Connection = connection;
            _getRevisions.Connection = connection;
            _getRevision.Connection = connection;
            _deleteHistory.Connection = connection;
            _countRevisions.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _newRevision.PrepareAsync();
            await _getRevisions.PrepareAsync();
            await _getRevision.PrepareAsync();
            await _deleteHistory.PrepareAsync();
            await _countRevisions.PrepareAsync();
        }
    }
}
