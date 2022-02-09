using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tomoe.Attributes;
using Tomoe.Models;

namespace Tomoe.Events
{
    public class PollEvent
    {
        [SubscribeToEvent(nameof(DiscordClient.ComponentInteractionCreated))]
        public static async Task PollAsync(DiscordClient client, ComponentInteractionCreateEventArgs componentInteractionCreateEventArgs)
        {
            if (componentInteractionCreateEventArgs.Id.StartsWith("poll\v", StringComparison.InvariantCultureIgnoreCase))
            {
                await componentInteractionCreateEventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true));

                DatabaseContext? database = Program.ServiceProvider.GetService<DatabaseContext>();
                if (database == null)
                {
                    await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("[Error]: Internal bot error, your vote was not casted. Please try again later."));
                    throw new InvalidOperationException("DatabaseContext is null!");
                }

                string[] idParts = componentInteractionCreateEventArgs.Id.Split('\v');
                if (idParts.Length != 3)
                {
                    await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("[Error]: Internal bot error, your vote was not casted. Please try again later."));
                    throw new InvalidOperationException("Invalid poll id!");
                }
                else if (!Guid.TryParse(idParts[1], out Guid pollId))
                {
                    await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("[Error]: Internal bot error, your vote was not casted. Please try again later."));
                    throw new InvalidOperationException("Invalid poll id!");
                }
                else
                {
                    PollModel? pollModel = database.Polls.FirstOrDefault(x => x.Id == pollId);
                    if (pollModel == null)
                    {
                        await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("[Error]: This poll has ended. Your vote was not casted."));
                        return;
                    }

                    (string? votedOn, ulong userId) = pollModel.Votes.Select(vote => vote.Value.Contains(componentInteractionCreateEventArgs.User.Id) ? (vote.Key, componentInteractionCreateEventArgs.User.Id) : (null, componentInteractionCreateEventArgs.User.Id)).FirstOrDefault(x => x.Key != null);
                    if (votedOn == null)
                    {
                        if (idParts[2] == "\tcancel")
                        {
                            await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("[Error]: You have not voted on this poll."));
                            return;
                        }
                        else
                        {
                            pollModel.Votes[idParts[2]] = pollModel.Votes[idParts[2]].Append(componentInteractionCreateEventArgs.User.Id).ToArray();
                            database.Entry(pollModel).State = EntityState.Modified;
                            await database.SaveChangesAsync();
                            await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Your vote has been casted!"));
                        }
                    }
                    else
                    {
                        if (idParts[2] == "\tcancel")
                        {
                            pollModel.Votes[votedOn] = pollModel.Votes[votedOn].Where(x => x != userId).ToArray();
                            database.Entry(pollModel).State = EntityState.Modified;
                            await database.SaveChangesAsync();
                            await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Your vote has been removed!"));
                        }
                        else
                        {
                            await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("[Error]: You have already voted on this poll."));
                        }
                    }
                }
            }
        }
    }
}