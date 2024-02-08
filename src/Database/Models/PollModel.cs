using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record PollModel
    {
        private static readonly SemaphoreSlim Semaphore = new(1, 1);
        private static readonly NpgsqlCommand CreateTable;
        private static readonly NpgsqlCommand CreatePoll;
        private static readonly NpgsqlCommand GetPoll;
        private static readonly NpgsqlCommand DeletePoll;

        public Ulid Id { get; init; }
        public ulong UserId { get; init; }
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong MessageId { get; init; }
        public string Title { get; init; } = null!;
        public IReadOnlyList<string> Options { get; init; } = null!;

        static PollModel()
        {
            CreateTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS polls(
                id UUID PRIMARY KEY,
                user_id BIGINT,
                guild_id BIGINT,
                channel_id BIGINT,
                message_id BIGINT,
                title TEXT NOT NULL,
                options TEXT[]
            );");

            CreatePoll = new NpgsqlCommand("INSERT INTO polls (id, user_id, guild_id, channel_id, message_id, title, options) VALUES (@id, @user_id, @guild_id, @channel_id, @message_id, @title, @options);");
            CreatePoll.Parameters.Add(new NpgsqlParameter<Ulid>("@id", NpgsqlDbType.Uuid));
            CreatePoll.Parameters.Add(new NpgsqlParameter<ulong>("@user_id", NpgsqlDbType.Bigint));
            CreatePoll.Parameters.Add(new NpgsqlParameter<ulong>("@guild_id", NpgsqlDbType.Bigint));
            CreatePoll.Parameters.Add(new NpgsqlParameter<ulong>("@channel_id", NpgsqlDbType.Bigint));
            CreatePoll.Parameters.Add(new NpgsqlParameter<ulong>("@message_id", NpgsqlDbType.Bigint));
            CreatePoll.Parameters.Add(new NpgsqlParameter<string>("@title", NpgsqlDbType.Text));
            CreatePoll.Parameters.Add(new NpgsqlParameter<IReadOnlyList<string>>("@options", NpgsqlDbType.Array | NpgsqlDbType.Text));

            GetPoll = new NpgsqlCommand("SELECT * FROM polls WHERE id = @id;");
            GetPoll.Parameters.Add(new NpgsqlParameter<Ulid>("@id", NpgsqlDbType.Uuid));

            DeletePoll = new NpgsqlCommand("DELETE FROM polls WHERE id = @id;");
            DeletePoll.Parameters.Add(new NpgsqlParameter<Ulid>("@id", NpgsqlDbType.Uuid));
        }

        public static async ValueTask<PollModel> CreatePollAsync(ulong userId, ulong guildId, ulong channelId, ulong messageId, string title, IReadOnlyList<string> options)
        {
            await Semaphore.WaitAsync();
            try
            {
                Ulid id = Ulid.NewUlid();
                CreatePoll.Parameters["@id"].Value = id;
                CreatePoll.Parameters["@user_id"].Value = userId;
                CreatePoll.Parameters["@guild_id"].Value = guildId;
                CreatePoll.Parameters["@channel_id"].Value = channelId;
                CreatePoll.Parameters["@message_id"].Value = messageId;
                CreatePoll.Parameters["@title"].Value = title;
                CreatePoll.Parameters["@options"].Value = options;
                await CreatePoll.ExecuteNonQueryAsync();
                return new PollModel
                {
                    Id = id,
                    UserId = userId,
                    GuildId = guildId,
                    ChannelId = channelId,
                    MessageId = messageId,
                    Title = title,
                    Options = options
                };
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask<PollModel?> GetPollAsync(Ulid id)
        {
            await Semaphore.WaitAsync();
            try
            {
                GetPoll.Parameters["@id"].Value = id;
                await using NpgsqlDataReader reader = await GetPoll.ExecuteReaderAsync();
                return !await reader.ReadAsync()
                    ? null
                    : new PollModel
                    {
                        Id = reader.GetFieldValue<Ulid>(0),
                        UserId = reader.GetFieldValue<ulong>(1),
                        GuildId = reader.GetFieldValue<ulong>(2),
                        ChannelId = reader.GetFieldValue<ulong>(3),
                        MessageId = reader.GetFieldValue<ulong>(4),
                        Title = reader.GetFieldValue<string>(5),
                        Options = reader.GetFieldValue<string[]>(6)
                    };
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask DeletePollAsync(Ulid id)
        {
            await Semaphore.WaitAsync();
            try
            {
                DeletePoll.Parameters["@id"].Value = id;
                await DeletePoll.ExecuteNonQueryAsync();
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            CreateTable.Connection = connection;
            CreatePoll.Connection = connection;
            GetPoll.Connection = connection;
            DeletePoll.Connection = connection;

            await CreateTable.ExecuteNonQueryAsync();
            await CreatePoll.PrepareAsync();
            await GetPoll.PrepareAsync();
            await DeletePoll.PrepareAsync();
        }
    }
}
