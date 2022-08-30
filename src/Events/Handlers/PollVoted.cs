using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Attributes;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class PollVoted
    {
        public ExpirableService<PollModel> PollService { get; init; } = null!;
        public ILogger<PollVoted> Logger { get; init; } = null!;
        public CancellationToken CancellationToken { get; init; }

        public PollVoted(ExpirableService<PollModel> pollService, ILogger<PollVoted> logger, CancellationTokenSource cancellationTokenSource)
        {
            ArgumentNullException.ThrowIfNull(pollService, nameof(pollService));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(cancellationTokenSource, nameof(cancellationTokenSource));

            PollService = pollService;
            Logger = logger;
            CancellationToken = cancellationTokenSource.Token;
        }

        [DiscordEventHandler(nameof(DiscordShardedClient.ComponentInteractionCreated))]
        public async Task PollVotedAsync(DiscordClient _, ComponentInteractionCreateEventArgs eventArgs)
        {
            if (!eventArgs.Id.StartsWith("poll\v"))
            {
                return;
            }

            string[] parts = eventArgs.Id.Split('\v');
            if (parts.Length != 3 || !Guid.TryParse(parts[1], out Guid pollId))
            {
                Logger.LogWarning("PollVotedAsync: Invalid poll id {PollId} for interaction {InteractionId}", parts[1], eventArgs.Id);
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Invalid poll id! This could be because the poll is old or I'm running outdated/incorrect code.").AsEphemeral());
                return;
            }

            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true));
            PollModel? poll = await PollService.TryGetItemAsync(pollId);
            if (poll == null)
            {
                await eventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("This poll is still being setup! Try sending your vote again, the poll should be done being setup by the time you finish this rather long response."));
                return;
            }


            PollVoteModel? pollVote = poll.Votes.FirstOrDefault(vote => vote.VoterId == eventArgs.User.Id);
            if (parts[2] == "remove")
            {
                if (pollVote != null)
                {
                    await poll.RemoveVoteAsync(eventArgs.User.Id, CancellationToken);
                    await eventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"Your vote for {pollVote.Option} has been removed."));
                }
                else
                {
                    await eventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("You haven't voted in this poll yet!"));
                }
            }
            else if (int.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out int pollOptionIndex))
            {
                PollOptionModel? pollOption = poll.Options.ElementAtOrDefault(pollOptionIndex);
                if (pollOption == null)
                {
                    Logger.LogError("Invalid poll option index {PollOptionIndex} on poll {PollId}", pollOptionIndex, poll.Id);
                    await eventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("I'm sorry, that poll option doesn't seem to exist. I think this is a bug, so I've already let the developers know."));
                    return;
                }
                else if (pollVote != null)
                {
                    await eventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"You've already voted for {pollVote.Option}!"));
                    return;
                }
                else
                {
                    await poll.AddVoteAsync(eventArgs.User.Id, pollOption.Id!.Value, CancellationToken);
                    await eventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent($"You've voted for {pollOption.Option}!"));
                }
            }
        }
    }
}
