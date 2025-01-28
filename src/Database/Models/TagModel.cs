using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record TagModel
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _createTag;
        private static readonly NpgsqlCommand _findTag;
        private static readonly NpgsqlCommand _findTagById;
        private static readonly NpgsqlCommand _updateTag;
        private static readonly NpgsqlCommand _deleteTag;
        private static readonly NpgsqlCommand _getTagContent;
        private static readonly NpgsqlCommand _tagExists;
        private static readonly NpgsqlCommand _getTags;
        private static readonly NpgsqlCommand _getTagsByOwner;

        public required Ulid Id { get; init; }
        public required string Name { get; init; }
        public required string Content { get; init; }
        public required ulong OwnerId { get; init; }
        public required ulong GuildId { get; init; }
        public required DateTimeOffset LastUpdatedAt { get; init; }
        public required ulong Uses { get; init; }

        static TagModel()
        {
            _createTable = new(@"CREATE TABLE IF NOT EXISTS tags(
                id UUID PRIMARY KEY,
                name TEXT NOT NULL,
                content TEXT NOT NULL,
                owner_id BIGINT NOT NULL,
                guild_id BIGINT NOT NULL,
                last_updated_at TIMESTAMPTZ NOT NULL,
                uses BIGINT DEFAULT 0
            );");

            _createTag = new(@"INSERT INTO tags (id, name, content, owner_id, guild_id, last_updated_at) VALUES (@id, @name, @content, @owner_id, @guild_id, @last_updated_at);");
            _createTag.Parameters.Add(new("@id", NpgsqlDbType.Uuid));
            _createTag.Parameters.Add(new("@name", NpgsqlDbType.Text));
            _createTag.Parameters.Add(new("@content", NpgsqlDbType.Text));
            _createTag.Parameters.Add(new("@owner_id", NpgsqlDbType.Bigint));
            _createTag.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));
            _createTag.Parameters.Add(new("@last_updated_at", NpgsqlDbType.TimestampTz));

            _findTag = new(@"SELECT * FROM tags WHERE name = @name AND guild_id = @guild_id;");
            _findTag.Parameters.Add(new("@name", NpgsqlDbType.Text));
            _findTag.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            _findTagById = new(@"SELECT * FROM tags WHERE id = @id;");
            _findTagById.Parameters.Add(new("@id", NpgsqlDbType.Uuid));

            _updateTag = new(@"UPDATE tags SET name = @name, content = @content, owner_id = @owner_id, last_updated_at = @last_updated_at WHERE id = @id;");
            _updateTag.Parameters.Add(new("@name", NpgsqlDbType.Text));
            _updateTag.Parameters.Add(new("@content", NpgsqlDbType.Text));
            _updateTag.Parameters.Add(new("@owner_id", NpgsqlDbType.Bigint));
            _updateTag.Parameters.Add(new("@last_updated_at", NpgsqlDbType.TimestampTz));
            _updateTag.Parameters.Add(new("@id", NpgsqlDbType.Uuid));

            _deleteTag = new(@"DELETE FROM tags WHERE id = @id;");
            _deleteTag.Parameters.Add(new("@id", NpgsqlDbType.Uuid));

            _getTagContent = new(@"UPDATE tags SET uses = uses + 1 WHERE name = @name AND guild_id = @guild_id RETURNING content;");
            _getTagContent.Parameters.Add(new("@name", NpgsqlDbType.Text));
            _getTagContent.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            _tagExists = new(@"SELECT EXISTS(SELECT 1 FROM tags WHERE name = @name AND guild_id = @guild_id);");
            _tagExists.Parameters.Add(new("@name", NpgsqlDbType.Text));
            _tagExists.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            _getTags = new(@"SELECT * FROM tags WHERE guild_id = @guild_id ORDER BY last_updated_at DESC");
            _getTags.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));

            _getTagsByOwner = new(@"SELECT * FROM tags WHERE owner_id = @owner_id AND guild_id = @guild_id ORDER BY last_updated_at DESC");
            _getTagsByOwner.Parameters.Add(new("@owner_id", NpgsqlDbType.Bigint));
            _getTagsByOwner.Parameters.Add(new("@guild_id", NpgsqlDbType.Bigint));
        }

        public static async ValueTask CreateAsync(Ulid id, string name, string content, ulong ownerId, ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _createTag.Parameters["@id"].Value = id.ToGuid();
                _createTag.Parameters["@name"].Value = name;
                _createTag.Parameters["@content"].Value = content;
                _createTag.Parameters["@owner_id"].Value = (long)ownerId;
                _createTag.Parameters["@guild_id"].Value = (long)guildId;
                _createTag.Parameters["@last_updated_at"].Value = id.Time;
                await _createTag.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<TagModel?> FindAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _findTagById.Parameters["@id"].Value = id.ToGuid();

                await using NpgsqlDataReader reader = await _findTagById.ExecuteReaderAsync();
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

        public static async ValueTask<TagModel?> FindAsync(string name, ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _findTag.Parameters["@name"].Value = name;
                _findTag.Parameters["@guild_id"].Value = (long)guildId;
                await using NpgsqlDataReader reader = await _findTag.ExecuteReaderAsync();
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

        public static async ValueTask UpdateAsync(Ulid id, string name, string content, ulong ownerId)
        {
            TagModel? tag = await FindAsync(id) ?? throw new InvalidOperationException("Tag does not exist.");
            await _semaphore.WaitAsync();
            try
            {
                _updateTag.Parameters["@name"].Value = name;
                _updateTag.Parameters["@content"].Value = content;
                _updateTag.Parameters["@owner_id"].Value = (long)ownerId;
                _updateTag.Parameters["@last_updated_at"].Value = DateTimeOffset.UtcNow;
                _updateTag.Parameters["@id"].Value = id.ToGuid();
                await _updateTag.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }

            await TagHistoryModel.CreateRevisionAsync(tag);
        }

        public static async ValueTask DeleteAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _deleteTag.Parameters["@id"].Value = id.ToGuid();
                await _deleteTag.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<string?> GetContentAsync(string name, ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getTagContent.Parameters["@name"].Value = name;
                _getTagContent.Parameters["@guild_id"].Value = (long)guildId;
                return (string?)await _getTagContent.ExecuteScalarAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<bool> ExistsAsync(string name, ulong guildId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _tagExists.Parameters["@name"].Value = name;
                _tagExists.Parameters["@guild_id"].Value = (long)guildId;
                return (bool)(await _tagExists.ExecuteScalarAsync())!;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async IAsyncEnumerable<TagModel> GetTagsAsync(ulong guildId, ulong ownerId)
        {
            await _semaphore.WaitAsync();
            try
            {
                NpgsqlCommand command;
                if (ownerId == 0)
                {
                    command = _getTags;
                    _getTags.Parameters["@guild_id"].Value = (long)guildId;
                }
                else
                {
                    command = _getTagsByOwner;
                    _getTagsByOwner.Parameters["@owner_id"].Value = (long)ownerId;
                    _getTagsByOwner.Parameters["@guild_id"].Value = (long)guildId;
                }

                await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                {
                    yield break;
                }

                while (await reader.ReadAsync())
                {
                    yield return new TagModel
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
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _createTag.Connection = connection;
            _findTag.Connection = connection;
            _findTagById.Connection = connection;
            _updateTag.Connection = connection;
            _deleteTag.Connection = connection;
            _getTagContent.Connection = connection;
            _tagExists.Connection = connection;
            _getTags.Connection = connection;
            _getTagsByOwner.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _createTag.PrepareAsync();
            await _findTag.PrepareAsync();
            await _findTagById.PrepareAsync();
            await _updateTag.PrepareAsync();
            await _deleteTag.PrepareAsync();
            await _getTagContent.PrepareAsync();
            await _tagExists.PrepareAsync();
            await _getTags.PrepareAsync();
            await _getTagsByOwner.PrepareAsync();
        }
    }
}
