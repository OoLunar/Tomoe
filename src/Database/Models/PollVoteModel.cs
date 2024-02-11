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
        private static readonly NpgsqlCommand _createOrEditVote;
        private static readonly NpgsqlCommand _getTotalVoteCount;
        private static readonly NpgsqlCommand _getOptionVoteCount;
        private static readonly NpgsqlCommand _removeVote;
        private static readonly NpgsqlCommand _clearVotes;

        public Ulid PollId { get; init; }
        public ulong UserId { get; init; }
        public int Option { get; init; }

        static PollVoteModel()
        {
            _createTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS poll_votes(
                poll_id TEXT NOT NULL,
                user_id BIGINT NOT NULL,
                option INT NOT NULL,
                CONSTRAINT pk_poll_votes PRIMARY KEY (poll_id, user_id)
            );");

            _createOrEditVote = new NpgsqlCommand("INSERT INTO poll_votes (poll_id, user_id, option) VALUES (@poll_id, @user_id, @option) ON CONFLICT (poll_id, user_id) DO UPDATE SET option = @option;");
            _createOrEditVote.Parameters.Add(new NpgsqlParameter("@poll_id", NpgsqlTypes.NpgsqlDbType.Text));
            _createOrEditVote.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _createOrEditVote.Parameters.Add(new NpgsqlParameter("@option", NpgsqlTypes.NpgsqlDbType.Integer));

            _getTotalVoteCount = new NpgsqlCommand("SELECT COUNT(*) FROM poll_votes WHERE poll_id = @poll_id;");
            _getTotalVoteCount.Parameters.Add(new NpgsqlParameter("@poll_id", NpgsqlTypes.NpgsqlDbType.Text));

            _getOptionVoteCount = new NpgsqlCommand("SELECT COUNT(*) FROM poll_votes WHERE poll_id = @poll_id AND option = @option;");
            _getOptionVoteCount.Parameters.Add(new NpgsqlParameter("@poll_id", NpgsqlTypes.NpgsqlDbType.Text));
            _getOptionVoteCount.Parameters.Add(new NpgsqlParameter("@option", NpgsqlTypes.NpgsqlDbType.Integer));

            _removeVote = new NpgsqlCommand("DELETE FROM poll_votes WHERE poll_id = @poll_id AND user_id = @user_id;");
            _removeVote.Parameters.Add(new NpgsqlParameter("@poll_id", NpgsqlTypes.NpgsqlDbType.Text));
            _removeVote.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlTypes.NpgsqlDbType.Bigint));

            _clearVotes = new NpgsqlCommand("DELETE FROM poll_votes WHERE poll_id = @poll_id;");
            _clearVotes.Parameters.Add(new NpgsqlParameter("@poll_id", NpgsqlTypes.NpgsqlDbType.Text));
        }

        public static async ValueTask VoteAsync(Ulid pollId, ulong userId, int option)
        {
            await _semaphore.WaitAsync();
            try
            {
                _createOrEditVote.Parameters["@poll_id"].Value = pollId.ToString();
                _createOrEditVote.Parameters["@user_id"].Value = (long)userId;
                _createOrEditVote.Parameters["@option"].Value = option;
                await _createOrEditVote.ExecuteNonQueryAsync();
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
                long count = (long)(await _getTotalVoteCount.ExecuteScalarAsync())!;
                return Unsafe.As<long, ulong>(ref count);
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

        public static async ValueTask RemoveVoteAsync(Ulid pollId, ulong userId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _removeVote.Parameters["@poll_id"].Value = pollId.ToString();
                _removeVote.Parameters["@user_id"].Value = (long)userId;
                await _removeVote.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask ClearVotesAsync(Ulid pollId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _clearVotes.Parameters["@poll_id"].Value = pollId.ToString();
                await _clearVotes.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _createOrEditVote.Connection = connection;
            _getTotalVoteCount.Connection = connection;
            _getOptionVoteCount.Connection = connection;
            _removeVote.Connection = connection;
            _clearVotes.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _createOrEditVote.PrepareAsync();
            await _getTotalVoteCount.PrepareAsync();
            await _getOptionVoteCount.PrepareAsync();
            await _removeVote.PrepareAsync();
            await _clearVotes.PrepareAsync();
        }
    }
}
