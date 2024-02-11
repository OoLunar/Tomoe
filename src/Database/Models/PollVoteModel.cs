using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record PollVoteModel
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _createVote;
        private static readonly NpgsqlCommand _editVote;
        private static readonly NpgsqlCommand _getTotalVoteCount;
        private static readonly NpgsqlCommand _getOptionVoteCount;

        public Ulid PollId { get; init; }
        public ulong UserId { get; init; }
        public int Option { get; init; }

        static PollVoteModel()
        {
            _createTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS poll_votes(
                poll_id TEXT NOT NULL PRIMARY KEY,
                user_id BIGINT NOT NULL,
                option INT NOT NULL
            );");

            _createVote = new NpgsqlCommand("INSERT INTO poll_votes (poll_id, user_id, option) VALUES (@poll_id, @user_id, @option);");
            _createVote.Parameters.Add(new NpgsqlParameter("@poll_id", NpgsqlTypes.NpgsqlDbType.Text));
            _createVote.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _createVote.Parameters.Add(new NpgsqlParameter("@option", NpgsqlTypes.NpgsqlDbType.Integer));

            _editVote = new NpgsqlCommand("UPDATE poll_votes SET option = @option WHERE poll_id = @poll_id AND user_id = @user_id;");
            _editVote.Parameters.Add(new NpgsqlParameter("@poll_id", NpgsqlTypes.NpgsqlDbType.Text));
            _editVote.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _editVote.Parameters.Add(new NpgsqlParameter("@option", NpgsqlTypes.NpgsqlDbType.Integer));

            _getTotalVoteCount = new NpgsqlCommand("SELECT COUNT(*) FROM poll_votes WHERE poll_id = @poll_id;");
            _getTotalVoteCount.Parameters.Add(new NpgsqlParameter("@poll_id", NpgsqlTypes.NpgsqlDbType.Text));

            _getOptionVoteCount = new NpgsqlCommand("SELECT COUNT(*) FROM poll_votes WHERE poll_id = @poll_id AND option = @option;");
            _getOptionVoteCount.Parameters.Add(new NpgsqlParameter("@poll_id", NpgsqlTypes.NpgsqlDbType.Text));
            _getOptionVoteCount.Parameters.Add(new NpgsqlParameter("@option", NpgsqlTypes.NpgsqlDbType.Integer));
        }

        public static async ValueTask CreateVoteAsync(Ulid pollId, ulong userId, int option)
        {
            await _semaphore.WaitAsync();
            try
            {
                _createVote.Parameters["@poll_id"].Value = pollId.ToString();
                _createVote.Parameters["@user_id"].Value = userId;
                _createVote.Parameters["@option"].Value = option;
                await _createVote.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask EditVoteAsync(Ulid pollId, ulong userId, int option)
        {
            await _semaphore.WaitAsync();
            try
            {
                _editVote.Parameters["@poll_id"].Value = pollId.ToString();
                _editVote.Parameters["@user_id"].Value = userId;
                _editVote.Parameters["@option"].Value = option;
                await _editVote.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<ulong> GetTotalVoteCountAsync(Ulid pollId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getTotalVoteCount.Parameters["@poll_id"].Value = pollId.ToString();
                return (ulong)(await _getTotalVoteCount.ExecuteScalarAsync())!;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<ulong> GetOptionVoteCountAsync(Ulid pollId, int option)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getOptionVoteCount.Parameters["@poll_id"].Value = pollId.ToString();
                _getOptionVoteCount.Parameters["@option"].Value = option;
                long count = (long)(await _getOptionVoteCount.ExecuteScalarAsync())!;
                return Unsafe.As<long, ulong>(ref count);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _createVote.Connection = connection;
            _editVote.Connection = connection;
            _getTotalVoteCount.Connection = connection;
            _getOptionVoteCount.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _createVote.PrepareAsync();
            await _editVote.PrepareAsync();
            await _getTotalVoteCount.PrepareAsync();
            await _getOptionVoteCount.PrepareAsync();
        }
    }
}
