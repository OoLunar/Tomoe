using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NpgsqlTypes;

namespace OoLunar.Tomoe.Database.Models
{
    [DatabaseModel]
    public sealed record PollModel : IExpirableModel<PollModel, Ulid>
    {
        public static string TableName => "polls";
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private static readonly NpgsqlCommand _createTable;
        private static readonly NpgsqlCommand _createPoll;
        private static readonly NpgsqlCommand _getPoll;
        private static readonly NpgsqlCommand _deletePoll;

        public Ulid Id { get; init; }
        public ulong UserId { get; init; }
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong MessageId { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }
        public string Title { get; init; } = null!;
        public IReadOnlyList<string> Options { get; init; } = null!;

        static PollModel()
        {
            _createTable = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS polls(
                id TEXT NOT NULL PRIMARY KEY,
                user_id BIGINT NOT NULL,
                guild_id BIGINT NOT NULL,
                channel_id BIGINT NOT NULL,
                message_id BIGINT NOT NULL,
                expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
                title TEXT NOT NULL,
                options TEXT[] NOT NULL
            );");

            _createPoll = new NpgsqlCommand("INSERT INTO polls (id, user_id, guild_id, channel_id, message_id, expires_at, title, options) VALUES (@id, @user_id, @guild_id, @channel_id, @message_id, @expires_at, @title, @options);");
            _createPoll.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Text));
            _createPoll.Parameters.Add(new NpgsqlParameter("@user_id", NpgsqlDbType.Bigint));
            _createPoll.Parameters.Add(new NpgsqlParameter("@guild_id", NpgsqlDbType.Bigint));
            _createPoll.Parameters.Add(new NpgsqlParameter("@channel_id", NpgsqlDbType.Bigint));
            _createPoll.Parameters.Add(new NpgsqlParameter("@message_id", NpgsqlDbType.Bigint));
            _createPoll.Parameters.Add(new NpgsqlParameter("@expires_at", NpgsqlDbType.TimestampTz));
            _createPoll.Parameters.Add(new NpgsqlParameter("@title", NpgsqlDbType.Text));
            _createPoll.Parameters.Add(new NpgsqlParameter("@options", NpgsqlDbType.Array | NpgsqlDbType.Text));

            _getPoll = new NpgsqlCommand("SELECT * FROM polls WHERE id = @id;");
            _getPoll.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Text));

            _deletePoll = new NpgsqlCommand("DELETE FROM polls WHERE id = @id;");
            _deletePoll.Parameters.Add(new NpgsqlParameter("@id", NpgsqlDbType.Text));
        }

        public static async ValueTask<PollModel> CreatePollAsync(Ulid id, ulong userId, ulong guildId, ulong channelId, ulong messageId, string title, DateTimeOffset expiresAt, IReadOnlyList<string> options)
        {
            await _semaphore.WaitAsync();
            try
            {
                _createPoll.Parameters["@id"].Value = id.ToString();
                _createPoll.Parameters["@user_id"].Value = (long)userId;
                _createPoll.Parameters["@guild_id"].Value = (long)guildId;
                _createPoll.Parameters["@channel_id"].Value = (long)channelId;
                _createPoll.Parameters["@message_id"].Value = (long)messageId;
                _createPoll.Parameters["@expires_at"].Value = expiresAt;
                _createPoll.Parameters["@title"].Value = title;
                _createPoll.Parameters["@options"].Value = options;
                await _createPoll.ExecuteNonQueryAsync();
                return new PollModel
                {
                    Id = id,
                    UserId = userId,
                    GuildId = guildId,
                    ChannelId = channelId,
                    MessageId = messageId,
                    ExpiresAt = expiresAt,
                    Title = title,
                    Options = options
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask<PollModel?> GetPollAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _getPoll.Parameters["@id"].Value = id.ToString();
                await using NpgsqlDataReader reader = await _getPoll.ExecuteReaderAsync();
                return await reader.ReadAsync() ? new PollModel
                {
                    Id = Ulid.Parse(reader.GetString(0), CultureInfo.InvariantCulture),
                    UserId = (ulong)reader.GetInt64(1),
                    GuildId = (ulong)reader.GetInt64(2),
                    ChannelId = (ulong)reader.GetInt64(3),
                    MessageId = (ulong)reader.GetInt64(4),
                    ExpiresAt = (DateTimeOffset)reader.GetDateTime(5),
                    Title = reader.GetString(6),
                    Options = reader.GetFieldValue<string[]>(7)
                } : null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask DeletePollAsync(Ulid id)
        {
            await _semaphore.WaitAsync();
            try
            {
                _deletePoll.Parameters["@id"].Value = id.ToString();
                await _deletePoll.ExecuteNonQueryAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public static async ValueTask PrepareAsync(NpgsqlConnection connection)
        {
            _createTable.Connection = connection;
            _createPoll.Connection = connection;
            _getPoll.Connection = connection;
            _deletePoll.Connection = connection;

            await _createTable.ExecuteNonQueryAsync();
            await _createPoll.PrepareAsync();
            await _getPoll.PrepareAsync();
            await _deletePoll.PrepareAsync();
        }

        public static bool TryParse(NpgsqlDataReader reader, [NotNullWhen(true)] out PollModel? expirable) => (expirable = new PollModel
        {
            Id = Ulid.Parse(reader.GetString(0), CultureInfo.InvariantCulture),
            UserId = (ulong)reader.GetInt64(1),
            GuildId = (ulong)reader.GetInt64(2),
            ChannelId = (ulong)reader.GetInt64(3),
            MessageId = (ulong)reader.GetInt64(4),
            ExpiresAt = (DateTimeOffset)reader.GetDateTime(5),
            Title = reader.GetString(6),
            Options = reader.GetFieldValue<string[]>(7)
        }) is not null;

        public static async ValueTask<bool> ExpireAsync(PollModel expirable, IServiceProvider serviceProvider)
        {
            DiscordClient? client = (await serviceProvider.GetRequiredService<Task<DiscordShardedClient>>()).GetShard(expirable.GuildId);
            if (client is null)
            {
                return true;
            }

            // This can happen when the bot was removed from the guild while a poll was still active.
            if (!client.Guilds.TryGetValue(expirable.GuildId, out DiscordGuild? guild))
            {
                return true;
            }
            // This can happen when the guild is unavailable due to an outage.
            else if (guild.IsUnavailable)
            {
                return false;
            }

            // This can happen when the channel was deleted while a poll was still active.
            if (!guild.Channels.TryGetValue(expirable.ChannelId, out DiscordChannel? channel))
            {
                return true;
            }

            // If the bot does not have permissions to send messages in the poll channel
            // DM the poll owner asking them to fix this.
            Permissions channelPermissionsForBot = channel.PermissionsFor(guild.CurrentMember);
            if ((!channel.IsThread && !channelPermissionsForBot.HasPermission(Permissions.SendMessages))
                || (channel.IsThread && !channelPermissionsForBot.HasPermission(Permissions.SendMessagesInThreads)))
            {
                DiscordMember member = await guild.GetMemberAsync(client.CurrentUser.Id);
                Permissions channelPermissionsForUser = channel.PermissionsFor(member);
                await member.SendMessageAsync($"I don't have permission to send messages in {channel.Mention} in {guild.Name}. I'm trying to send the results to a poll but I can't! {(channelPermissionsForUser.HasPermission(Permissions.ManageChannels) || channelPermissionsForUser.HasPermission(Permissions.ManageRoles)
                    ? "Could you please fix this?"
                    : "Please let an administrator know so they can fix this.")}");

                return false;
            }

            // Calculate the winners
            Dictionary<string, ulong> votes = [];
            for (int i = 0; i < expirable.Options.Count; i++)
            {
                votes[expirable.Options[i]] = await PollVoteModel.GetOptionVoteCountAsync(expirable.Id, i);
            }

            // Order the votes by the amount of votes they have
            votes = votes.OrderByDescending(x => x.Value).ToDictionary();
            DiscordMessageBuilder messageBuilder = new()
            {
                // We... don't talk about this. Improvements are welcome.
                Content = votes.Count switch
                {
                    // We don't need to account for 0 votes due to the above check.
                    0 or _ when votes.First().Value == 0 => "The winner is... Nobody! There weren't any votes...",
                    // The winner is Minecraft with 14,012 votes!
                    1 => $"The winner is {votes.First().Key} with {votes.First().Value:N0} vote{(votes.First().Value == 1 ? null : "s")}!",
                    // We have a two way tie between Minecraft and Terraria. Both have 1 vote!
                    2 => $"We have a two way tie between {votes.First().Key} and {votes.ElementAt(1).Key}. Both have {votes.First().Value:N0} vote{(votes.First().Value == 1 ? null : "s")}!",
                    // We have a six way tie, each with 14,012 votes! Nobody could decide between Minecraft, Terraria, Hollow Knight, Mario Kart Wii, Wii Sports and Smash Bros.!
                    _ => $"We have a {votes.Count.ToWords()} way tie, each with {votes.First().Value:N0} vote{(votes.First().Value == 1 ? null : "s")}! Nobody could decide between {votes.Select(x => x.Key).Humanize()}."
                }
            };

            messageBuilder.WithReply(expirable.MessageId);

            // Ensure we only add the embed when we have permission to do so.
            if (channelPermissionsForBot.HasPermission(Permissions.EmbedLinks))
            {
                DiscordEmbedBuilder embedBuilder = new()
                {
                    Color = new DiscordColor(0x6B73DB),
                    Description = "The following options were available with their total vote count below:",
                    Footer = new()
                    {
                        Text = "Poll was created "
                    },
                    Timestamp = expirable.Id.Time
                };

                foreach ((string option, ulong count) in votes)
                {
                    embedBuilder.AddField(option, count.ToString("N0", CultureInfo.InvariantCulture), true);
                }

                messageBuilder.AddEmbed(embedBuilder);
            }

            DiscordMessage winningMessage = await channel.SendMessageAsync(messageBuilder);
            DiscordMessage? message = await channel.GetMessageAsync(expirable.MessageId);
            DiscordMessageBuilder builder = new(message);
            if (message.Components is null)
            {
                return true;
            }

            List<DiscordActionRowComponent> actionRows = [];
            for (int i = 0; i < message.Components.Count; i++)
            {
                DiscordActionRowComponent actionRow = builder.Components[i];
                for (int j = 0; j < actionRow.Components.Count; j++)
                {
                    DiscordComponent component = actionRow.Components[j];
                    if (component is not DiscordButtonComponent button)
                    {
                        continue;
                    }

                    if (button.Style == ButtonStyle.Danger)
                    {
                        actionRow = new DiscordActionRowComponent(actionRow.Components
                            // Take all the buttons before the delete button
                            .Take(j)
                            // Replace the delete button with the jumplink button
                            .Append(new DiscordLinkButtonComponent(winningMessage.JumpLink.ToString(), "View Results"))
                        );
                    }
                    else
                    {
                        button.Disable();
                    }
                }

                actionRows.Add(actionRow);
            }

            builder.ClearComponents();
            builder.AddComponents(actionRows);
            await message.ModifyAsync(builder);
            return true;
        }
    }
}
