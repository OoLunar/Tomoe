using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record ReminderModel : IExpirableModel<ReminderModel, Ulid>
    {
        public static string TableName => "reminders";
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _createReminder;
        private static readonly NpgsqlCommand _deleteReminder;
        private static readonly NpgsqlCommand _getReminder;
        private static readonly NpgsqlCommand _listReminders;

        public required Ulid Id { get; init; }
        public required ulong UserId { get; init; }
        public required ulong GuildId { get; init; }
        public required ulong ChannelId { get; init; }
        public required ulong MessageId { get; init; }
        public required DateTimeOffset ExpiresAt { get; init; }
        public required ReminderType Type { get; init; }
        public required TimeSpan Interval { get; init; }
        public required string Content { get; init; }

        static ReminderModel()
        {
            _createTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS reminders(
                id TEXT NOT NULL,
                user_id BIGINT NOT NULL,
                guild_id BIGINT NOT NULL,
                channel_id BIGINT NOT NULL,
                message_id BIGINT NOT NULL,
                expires_at TIMESTAMPTZ NOT NULL,
                type SMALLINT NOT NULL,
                interval INTERVAL NOT NULL,
                content TEXT NOT NULL
            );");

            _createReminder = new NpgsqlCommand(@"INSERT INTO reminders (id, user_id, guild_id, channel_id, message_id, expires_at, type, interval, content) VALUES (@id, @user_id, @guild_id, @channel_id, @message_id, @expires_at, @type, @interval, @content);");
            _createReminder.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Text));
            _createReminder.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _createReminder.Parameters.Add(new NpgsqlParameter("guild_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _createReminder.Parameters.Add(new NpgsqlParameter("channel_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _createReminder.Parameters.Add(new NpgsqlParameter("message_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _createReminder.Parameters.Add(new NpgsqlParameter("expires_at", NpgsqlTypes.NpgsqlDbType.TimestampTz));
            _createReminder.Parameters.Add(new NpgsqlParameter("type", NpgsqlTypes.NpgsqlDbType.Smallint));
            _createReminder.Parameters.Add(new NpgsqlParameter("interval", NpgsqlTypes.NpgsqlDbType.Interval));
            _createReminder.Parameters.Add(new NpgsqlParameter("content", NpgsqlTypes.NpgsqlDbType.Text));

            _deleteReminder = new NpgsqlCommand(@"DELETE FROM reminders WHERE id = @id;");
            _deleteReminder.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Text));

            _getReminder = new NpgsqlCommand(@"SELECT * FROM reminders WHERE id = @id;");
            _getReminder.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Text));

            _listReminders = new NpgsqlCommand(@"SELECT * FROM reminders WHERE user_id = @user_id LIMIT 1 OFFSET @offset;");
            _listReminders.Parameters.Add(new NpgsqlParameter("user_id", NpgsqlTypes.NpgsqlDbType.Bigint));
            _listReminders.Parameters.Add(new NpgsqlParameter("offset", NpgsqlTypes.NpgsqlDbType.Bigint));
        }

        public static async ValueTask<ReminderModel> CreateAsync(Ulid id, ulong userId, ulong guildId, ulong channelId, ulong messageId, DateTimeOffset expiresAt, ReminderType type, TimeSpan interval, string content)
        {
            await _semaphore.WaitAsync();
            try
            {
                _createReminder.Parameters["id"].Value = id.ToString();
                _createReminder.Parameters["user_id"].Value = (long)userId;
                _createReminder.Parameters["guild_id"].Value = (long)guildId;
                _createReminder.Parameters["channel_id"].Value = (long)channelId;
                _createReminder.Parameters["message_id"].Value = (long)messageId;
                _createReminder.Parameters["expires_at"].Value = expiresAt;
                _createReminder.Parameters["type"].Value = (short)type;
                _createReminder.Parameters["interval"].Value = interval;
                _createReminder.Parameters["content"].Value = content;
                await _createReminder.ExecuteNonQueryAsync();
                return new ReminderModel
                {
                    Id = id,
                    UserId = userId,
                    GuildId = guildId,
                    ChannelId = channelId,
                    MessageId = messageId,
                    ExpiresAt = expiresAt,
                    Type = type,
                    Interval = interval,
                    Content = content
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask DeleteAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _deleteReminder.Parameters["id"].Value = id.ToString();
                await _deleteReminder.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<ReminderModel?> GetAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getReminder.Parameters["id"].Value = id.ToString();
                await using NpgsqlDataReader reader = await _getReminder.ExecuteReaderAsync();
                return TryParse(reader, out ReminderModel? reminder) ? reminder : null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async IAsyncEnumerable<ReminderModel> ListAsync(ulong userId)
        {
            long index = 0;
            while (true)
            {
                await _semaphore.WaitAsync();
                try
                {
                    _listReminders.Parameters["user_id"].Value = (long)userId;
                    _listReminders.Parameters["offset"].Value = index;

                    await using NpgsqlDataReader reader = await _listReminders.ExecuteReaderAsync();
                    if (!reader.HasRows || !await reader.ReadAsync() || !TryParse(reader, out ReminderModel? reminder))
                    {
                        break;
                    }

                    index++;
                    yield return reminder;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _createReminder.Connection = connection;
            _deleteReminder.Connection = connection;
            _getReminder.Connection = connection;
            _listReminders.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _createReminder.PrepareAsync();
            await _deleteReminder.PrepareAsync();
            await _getReminder.PrepareAsync();
            await _listReminders.PrepareAsync();
        }

        public static bool TryParse(NpgsqlDataReader reader, [NotNullWhen(true)] out ReminderModel? expirable) => (expirable = new()
        {
            Id = Ulid.Parse(reader.GetString(0)),
            UserId = (ulong)reader.GetInt64(1),
            GuildId = (ulong)reader.GetInt64(2),
            ChannelId = (ulong)reader.GetInt64(3),
            MessageId = (ulong)reader.GetInt64(4),
            ExpiresAt = (DateTimeOffset)reader.GetDateTime(5),
            Type = (ReminderType)reader.GetByte(6),
            Interval = reader.GetTimeSpan(7),
            Content = reader.GetString(8)
        }) is not null;

        public static async ValueTask<bool> ExpireAsync(ReminderModel expirable, IServiceProvider serviceProvider)
        {
            DiscordShardedClient? shardedClient = serviceProvider.GetRequiredService<DiscordShardedClient>();
            if (shardedClient is null)
            {
                // If this happens we'll have much bigger problems but
                // Don't remove the reminder from the database.
                return false;
            }

            // Since we're not manually sharding, client should never be null as the shard is calculated from the guild id.
            DiscordClient? client = shardedClient.GetShard(expirable.GuildId);
            if (client is null || expirable.GuildId == 0 || !client.Guilds.TryGetValue(expirable.GuildId, out DiscordGuild? guild) || guild.IsUnavailable)
            {
                // If it does though, try finding the user through a separate guild and DM'ing them.
                return await DmUserAsync(shardedClient, expirable);
            }

            // Make sure the user is still in the guild.
            GuildMemberModel? memberModel = await GuildMemberModel.FindMemberAsync(expirable.UserId, guild.Id);
            if (memberModel is null || memberModel.State.HasFlag(GuildMemberState.Absent) || !guild.Channels.TryGetValue(expirable.ChannelId, out DiscordChannel? channel))
            {
                return await DmUserAsync(shardedClient, expirable);
            }

            // Double check if the bot has permissions to send messages in the channel.
            DiscordPermissions channelPermissionsForBot = channel.PermissionsFor(guild.CurrentMember);

            // If the channel is a normal text channel and we cannot send messages
            // or if the channel is a thread and we cannot send messages in threads.
            if ((!channel.IsThread && !channelPermissionsForBot.HasPermission(DiscordPermissions.SendMessages))
                || (channel.IsThread && !channelPermissionsForBot.HasPermission(DiscordPermissions.SendMessagesInThreads)))
            {
                return await DmUserAsync(shardedClient, expirable);
            }

            await channel.SendMessageAsync(CreateReminderMessage(expirable, expirable.GuildId));
            return true;
        }

        private static async ValueTask<bool> DmUserAsync(DiscordShardedClient shardedClient, ReminderModel reminderModel)
        {
            IReadOnlyList<ulong> guildIds = await GuildMemberModel.FindMutualGuildsAsync(reminderModel.UserId);
            if (guildIds.Count == 0)
            {
                // If the user is not in any guild the bot is in, toss the reminder.
                return true;
            }

            // If the user is in a guild the bot is in, try to DM them.
            bool guildUnavailable = false;
            foreach (ulong guildId in guildIds)
            {
                DiscordClient? client = shardedClient.GetShard(guildId);
                if (client is null || !client.Guilds.TryGetValue(guildId, out DiscordGuild? guild))
                {
                    continue;
                }
                else if (guild.IsUnavailable)
                {
                    guildUnavailable = true;
                    continue;
                }

                try
                {
                    // By default the method will check cache first. If the cache doesn't have the user
                    // Then it'll make a rest request to the API.
                    // We're pretty confident that the user is in the guild since event handlers should
                    // always keep our database up-to-date, but surround it with a try-catch for when a race condition occurs.
                    DiscordMember member = await guild.GetMemberAsync(reminderModel.UserId);
                    await member.SendMessageAsync(CreateReminderMessage(reminderModel, guildId));
                }
                catch (DiscordException)
                {
                    continue;
                }

                // If the message was sent successfully, remove the reminder from the database.
                return true;
            }

            // If the guild was unavailable, we may still be able to DM the user another time.
            // Keep the reminder in the database if there are guilds unavailable, otherwise remove it.
            return !guildUnavailable;
        }

        private static DiscordMessageBuilder CreateReminderMessage(ReminderModel reminderModel, ulong guildId)
        {
            DiscordEmbedBuilder embedBuilder = new();
            embedBuilder.WithTitle($"A reminder from {Formatter.Timestamp(reminderModel.Id.Time)}!");
            embedBuilder.WithDescription(reminderModel.Content);
            embedBuilder.AddField("Created At", Formatter.Timestamp(reminderModel.Id.Time, TimestampFormat.LongDateTime), true);
            embedBuilder.AddField("Expires At", Formatter.Timestamp(reminderModel.ExpiresAt, TimestampFormat.LongDateTime), true);
            embedBuilder.AddField("Duration", (reminderModel.ExpiresAt - reminderModel.Id.Time).Humanize(2), true);

            DiscordMessageBuilder builder = new();
            builder.WithAllowedMention(new UserMention(reminderModel.UserId));
            builder.WithContent($"Hey <@{reminderModel.UserId}>, don't forget about this!");
            if (reminderModel.GuildId != 0 && reminderModel.GuildId != guildId)
            {
                embedBuilder.WithFooter("I tried to reach out to you in the server you set the reminder in, but I couldn't. I hope you don't mind if I DM you instead!");
            }
            else if (reminderModel.MessageId != 0)
            {
                builder.WithReply(reminderModel.MessageId);
            }

            builder.AddEmbed(embedBuilder);
            return builder;
        }
    }
}
