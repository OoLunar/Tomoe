using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Interfaces;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Database.Models
{
    public sealed class PollModel : IExpirable<PollModel>
    {
        public Guid Id { get; init; }
        public string Question { get; init; }
        public string[] Options { get; init; } = Array.Empty<string>();

        /// <summary>
        /// A dictionary of user IDs and the option they voted for. The values are used to index into the Options array.
        /// </summary>
        [Column(TypeName = "json")]
        public Dictionary<ulong, int> Votes { get; init; } = new();
        public DateTime ExpiresAt { get; set; }

        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong MessageId { get; init; }

        [NotMapped]
        private readonly Lazy<Dictionary<string, int>> _winners;
        [NotMapped]
        private Dictionary<string, int> Winners => _winners.Value;

        public PollModel() => _winners = new(() =>
        {
            int[] optionVotes = new int[Options.Length];

            // Use LINQ to increment the count for each option
            foreach (KeyValuePair<ulong, int> kvp in Votes)
            {
                optionVotes[kvp.Value]++;
            }

            // Use LINQ to create a dictionary of option votes and sort it by value
            return Options.Select((option, i) => new KeyValuePair<string, int>(option, optionVotes[i]))
                .OrderByDescending(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        });

        public PollModel(Guid id, string question, IEnumerable<string> options, DateTime expiresAt, ulong? guildId, ulong channelId, ulong messageId) : base()
        {
            Id = id;
            Question = question;
            Options = options.ToArray();
            Votes = new();
            ExpiresAt = expiresAt;
            GuildId = guildId ?? 0;
            ChannelId = channelId;
            MessageId = messageId;
        }

        public async Task ExpireAsync(IServiceProvider serviceProvider)
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            ILogger<PollModel> logger = serviceProvider.GetRequiredService<ILogger<PollModel>>();
            ExpirableService<PollModel> expirableService = serviceProvider.GetRequiredService<ExpirableService<PollModel>>();
            PollService pollService = serviceProvider.GetRequiredService<PollService>();
            DiscordShardedClient shardedClient = serviceProvider.GetRequiredService<DiscordShardedClient>();
            DiscordClient client = GuildId == 0 ? shardedClient.ShardClients[0] : shardedClient.GetShard(GuildId);
            DiscordChannel channel;

            if (GuildId == 0)
            {
                // Dm channel
                channel = await client.GetChannelAsync(ChannelId);
            }
            else
            {
                if (!client.Guilds.TryGetValue(GuildId, out DiscordGuild? guild))
                {
                    logger.LogWarning("Guild {GuildId} not found. Deleting the poll.", GuildId);
                    return;
                }
                else if (guild.IsUnavailable)
                {
                    logger.LogWarning("Guild {GuildId} is unavailable. Postponing it for 10 minutes.", GuildId);
                    ExpiresAt = ExpiresAt.AddMinutes(10);
                    await expirableService.UpdateAsync(this);
                    return;
                }

                channel = guild.GetChannel(ChannelId);
            }

            if (channel == null)
            {
                logger.LogWarning("Channel {ChannelId} not found. Deleting the poll.", ChannelId);
                return;
            }

            Dictionary<string, int> winners = Winners.TakeWhile(x => x.Value == Winners.First().Value).ToDictionary(x => x.Key, x => x.Value);
            DiscordMessageBuilder messageBuilder = new()
            {
                // We... don't talk about this. Improvements are welcome.
                Content = winners.Count switch
                {
                    // We don't need to account for 0 votes due to the above check.
                    0 or _ when winners.First().Value == 0 => "The winner is... Nobody! There weren't any votes...",
                    1 => $"The winner is {winners.First().Key} with {winners.First().Value:N0} vote{(winners.First().Value == 1 ? null : "s")}!", // The winner is Minecraft with 14,012 votes!
                    2 => $"We have a two way tie between {winners.First().Key} and {winners.ElementAt(1).Key}. Both have {winners.First().Value:N0} vote{(winners.First().Value == 1 ? null : "s")}!", // We have a two way tie between Minecraft and Terraria. Both have 1 vote!
                    _ => $"We have a {winners.Count.ToWords()} way tie, each with {winners.First().Value:N0} vote{(winners.First().Value == 1 ? null : "s")}! Nobody could decide between {winners.Select(x => x.Key).Humanize()}." // We have a six way tie, each with 14,012 votes! Nobody could decide between Minecraft, Terraria, Hollow Knight, Mario Kart Wii, Wii Sports and Smash Bros.!
                }
            };

            messageBuilder.WithReply(MessageId);
            messageBuilder.WithAllowedMentions(Mentions.None);

            if (winners.First().Value != 0)
            {
                messageBuilder.AddFile("image.png", GenerateBarGraph());
            }

            try
            {
                await channel.SendMessageAsync(messageBuilder);
                logger.LogInformation("Sent poll results for poll {PollId}.", Id);
            }
            catch (DiscordException error) when (error.WebResponse.ResponseCode >= 500)
            {
                logger.LogWarning(error, "Failed to send poll results. Postponing it for 10 minutes.");
                ExpiresAt = ExpiresAt.AddMinutes(10);
                await expirableService.UpdateAsync(this);
            }

            await pollService.RemovePollAsync(Id);
        }

        public Stream GenerateBarGraph()
        {
            // Create a dictionary to count the votes for each option
            Dictionary<int, int> optionVotes = Options.Select((_, i) => new KeyValuePair<int, int>(i, 0)).ToDictionary(x => x.Key, x => x.Value);

            // Iterate through the Votes dictionary and increment the count for each option
            foreach ((ulong userId, int option) in Votes)
            {
                optionVotes[option]++;
            }

            // Sort the dictionary by the value (the number of votes)
            optionVotes = new(optionVotes.OrderByDescending(x => x.Value));
            int maxVotes = optionVotes.First().Value;

            ISeries[] bars = new[] {
                new ColumnSeries<int>
                {
                    Values = optionVotes.Values
                }
            };

            SKCartesianChart barChart = new()
            {
                Width = 600,
                Height = 400,
                Series = bars,
                XAxes = new Axis[]
                {
                    new()
                    {
                        Labels = optionVotes.Keys.Select(x => Options[x]).ToArray(),
                        ForceStepToMin = true
                    }
                }
            };

            return barChart.GetImage().Encode().AsStream();
        }
    }
}
