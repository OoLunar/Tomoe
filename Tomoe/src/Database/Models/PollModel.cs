using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
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
        public string[] Options { get; init; }

        [Column(TypeName = "json")]
        public Dictionary<ulong, int> Votes { get; init; }
        public DateTime ExpiresAt { get; set; }

        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong MessageId { get; init; }

        [NotMapped]
        public SemaphoreSlim IsExecuting { get; } = new(1, 1);
        [NotMapped]
        public bool HasExecuted { get; set; }

        public PollModel() { }
        public PollModel(Guid id, string question, IEnumerable<string> options, DateTime expiresAt, ulong? guildId, ulong channelId, ulong messageId)
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

            Dictionary<string, int> winners = Votes
                .GroupBy(vote => vote.Value) // Group by vote
                .OrderByDescending(group => group.Count()) // Order by vote count
                .GroupBy(group => group) // Grab the top X winners
                .Where(group => group.Any()) // Filter out the rest
                .Select(group => group.Key) // Grab the key
                .ToDictionary(vote => Options[vote.Key], vote => Votes.Where(voter => voter.Value == vote.Key).Count()); // Grab the option

            DiscordMessageBuilder messageBuilder = new()
            {
                // We... don't talk about this. Improvements are welcome.
                Content = winners.Count switch
                {
                    // We don't need to account for 0 votes due to the above check.
                    0 => "The winner is... Nobody! There weren't any votes...",
                    1 => $"The winner is {winners.First().Key} with {winners.First().Value:N0} vote{(winners.First().Value == 1 ? null : "s")}!", // The winner is Minecraft with 14,012 votes!
                    2 => $"We have a two way tie between {winners.First().Key} and {winners.ElementAt(1).Key}. Both have {winners.First().Value:N0} vote{(winners.First().Value == 1 ? null : "s")}!", // We have a two way tie between Minecraft and Terraria. Both have 1 vote!
                    _ => $"We have a {winners.Count.ToWords()} way tie, each with {winners.First().Value:N0} vote{(winners.First().Value == 1 ? null : "s")}! Nobody could decide between {winners.Select(x => x.Key).Humanize()}." // We have a six way tie, each with 14,012 votes! Nobody could decide between Minecraft, Terraria, Hollow Knight, Mario Kart Wii, Wii Sports and Smash Bros.!
                }
            };

            messageBuilder.WithReply(MessageId);
            messageBuilder.WithAllowedMentions(Mentions.None);

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

        private void GenerateBarGraph()
        {
            // Using Options as the bars, Votes as the data, generate the bar graph with ImageSharp

        }
    }
}
