using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class PollSubmittedEventHandler : IEventHandler<InteractionCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, InteractionCreatedEventArgs eventArgs)
        {
            if (eventArgs.Interaction.Type != DiscordInteractionType.Component)
            {
                return;
            }

            string[] args = eventArgs.Interaction.Data.CustomId.Split(':');
            if (args.Length < 2 || args[0] != "poll" || !Ulid.TryParse(args[1], CultureInfo.InvariantCulture, out Ulid pollId))
            {
                return;
            }
            else if (!await PollModel.PollExistsAsync(pollId))
            {
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "This poll has ended, your most recent vote wasn't counted.",
                    IsEphemeral = true
                });

                return;
            }

            if (args.Length == 2)
            {
                await PollVoteModel.RemoveVoteAsync(pollId, eventArgs.Interaction.User.Id);
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "Your vote has been removed.",
                    IsEphemeral = true
                });
            }
            else if (int.TryParse(args[2], out int option))
            {
                await PollVoteModel.VoteAsync(pollId, eventArgs.Interaction.User.Id, option);
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "You vote has been recorded.",
                    IsEphemeral = true
                });
            }
        }
    }
}
