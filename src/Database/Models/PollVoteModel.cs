using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record PollVoteModel
    {
        private static readonly SemaphoreSlim Semaphore = new(1, 1);
        private static readonly NpgsqlCommand CreateTable;
        private static readonly NpgsqlCommand CreateVote;
        private static readonly NpgsqlCommand EditVote;
        private static readonly NpgsqlCommand GetTotalVoteCount;
        private static readonly NpgsqlCommand GetOptionVoteCount;

        public Ulid PollId { get; init; }
        public ulong UserId { get; init; }
        public int Option { get; init; }

        static PollVoteModel()
        {
            CreateTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS poll_votes(
                poll_id UUID PRIMARY KEY,
                user_id BIGINT,
                option INT
            );");

            CreateVote = new NpgsqlCommand("INSERT INTO poll_votes (poll_id, user_id, option) VALUES (@poll_id, @user_id, @option);");
            CreateVote.Parameters.Add(new NpgsqlParameter<Ulid>("@poll_id", NpgsqlTypes.NpgsqlDbType.Uuid));
            CreateVote.Parameters.Add(new NpgsqlParameter<ulong>("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            CreateVote.Parameters.Add(new NpgsqlParameter<int>("@option", NpgsqlTypes.NpgsqlDbType.Integer));

            EditVote = new NpgsqlCommand("UPDATE poll_votes SET option = @option WHERE poll_id = @poll_id AND user_id = @user_id;");
            EditVote.Parameters.Add(new NpgsqlParameter<Ulid>("@poll_id", NpgsqlTypes.NpgsqlDbType.Uuid));
            EditVote.Parameters.Add(new NpgsqlParameter<ulong>("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            EditVote.Parameters.Add(new NpgsqlParameter<int>("@option", NpgsqlTypes.NpgsqlDbType.Integer));

            GetTotalVoteCount = new NpgsqlCommand("SELECT COUNT(*) FROM poll_votes WHERE poll_id = @poll_id;");
            GetTotalVoteCount.Parameters.Add(new NpgsqlParameter<Ulid>("@poll_id", NpgsqlTypes.NpgsqlDbType.Uuid));

            GetOptionVoteCount = new NpgsqlCommand("SELECT COUNT(*) FROM poll_votes WHERE poll_id = @poll_id AND option = @option;");
            GetOptionVoteCount.Parameters.Add(new NpgsqlParameter<Ulid>("@poll_id", NpgsqlTypes.NpgsqlDbType.Uuid));
            GetOptionVoteCount.Parameters.Add(new NpgsqlParameter<int>("@option", NpgsqlTypes.NpgsqlDbType.Integer));
        }

        public static async ValueTask CreateVoteAsync(Ulid pollId, ulong userId, int option)
        {
            await Semaphore.WaitAsync();
            try
            {
                CreateVote.Parameters["@poll_id"].Value = pollId;
                CreateVote.Parameters["@user_id"].Value = userId;
                CreateVote.Parameters["@option"].Value = option;
                await CreateVote.ExecuteNonQueryAsync();
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask EditVoteAsync(Ulid pollId, ulong userId, int option)
        {
            await Semaphore.WaitAsync();
            try
            {
                EditVote.Parameters["@poll_id"].Value = pollId;
                EditVote.Parameters["@user_id"].Value = userId;
                EditVote.Parameters["@option"].Value = option;
                await EditVote.ExecuteNonQueryAsync();
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask<ulong> GetTotalVoteCountAsync(Ulid pollId)
        {
            await Semaphore.WaitAsync();
            try
            {
                GetTotalVoteCount.Parameters["@poll_id"].Value = pollId;
                return (ulong)(await GetTotalVoteCount.ExecuteScalarAsync())!;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask<ulong> GetOptionVoteCountAsync(Ulid pollId, int option)
        {
            await Semaphore.WaitAsync();
            try
            {
                GetOptionVoteCount.Parameters["@poll_id"].Value = pollId;
                GetOptionVoteCount.Parameters["@option"].Value = option;
                return (ulong)(await GetOptionVoteCount.ExecuteScalarAsync())!;
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            CreateTable.Connection = connection;
            CreateVote.Connection = connection;
            EditVote.Connection = connection;
            GetTotalVoteCount.Connection = connection;
            GetOptionVoteCount.Connection = connection;

            await CreateTable.ExecuteNonQueryAsync();
            await CreateVote.PrepareAsync();
            await EditVote.PrepareAsync();
            await GetTotalVoteCount.PrepareAsync();
            await GetOptionVoteCount.PrepareAsync();
        }
    }
}
