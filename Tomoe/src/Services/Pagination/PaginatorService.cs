using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Events;

namespace OoLunar.Tomoe.Services.Pagination
{
    public sealed class PaginatorService
    {
        private ConcurrentDictionary<Guid, Paginator> CurrentPaginators { get; init; } = new();
        private PeriodicTimer CleanupTimer { get; init; } = new(TimeSpan.FromSeconds(1));
        private TimeSpan PaginatorTimeout { get; init; } = TimeSpan.FromSeconds(30);

        public PaginatorService(IConfiguration configuration)
        {
            PaginatorTimeout = configuration.GetValue("discord:paginator_timeout", TimeSpan.FromSeconds(30));
            _ = StartExpiringPaginatorsAsync();
        }

        public Paginator CreatePaginator(IEnumerable<Page> pages, DiscordUser author)
        {
            ArgumentNullException.ThrowIfNull(author, nameof(author));
            ArgumentNullException.ThrowIfNull(pages, nameof(pages));

            Guid id = Guid.NewGuid();
            while (CurrentPaginators.ContainsKey(id))
            {
                id = Guid.NewGuid();
            }

            Paginator paginator = new(id, pages, author);
            CurrentPaginators.AddOrUpdate(paginator.Id, paginator, (key, value) => paginator);
            return paginator;
        }

        public Paginator CreatePaginator(Paginator existingPaginator, DiscordUser newAuthor, DiscordMessage? newMessage = null)
        {
            ArgumentNullException.ThrowIfNull(existingPaginator, nameof(existingPaginator));

            Guid id = Guid.NewGuid();
            while (CurrentPaginators.ContainsKey(id))
            {
                id = Guid.NewGuid();
            }

            Paginator paginator = new(id, existingPaginator, newAuthor) { CurrentMessage = newMessage };
            CurrentPaginators.AddOrUpdate(paginator.Id, paginator, (key, value) => paginator);
            return paginator;
        }

        public Paginator? GetPaginator(Guid paginatorId) => CurrentPaginators.TryGetValue(paginatorId, out Paginator? paginator) ? paginator : null;

        public async Task<bool> RemovePaginatorAsync(Guid paginatorId, bool editMessage = false)
        {
            if (CurrentPaginators.TryRemove(paginatorId, out Paginator? paginator) // If the paginator exists
                && paginator != null // And isn't null
                && editMessage // And we're supposed to edit the message
                && paginator.CurrentMessage != null // And has sent a message
                && (!paginator.CurrentMessage.Flags?.HasFlag(MessageFlags.Ephemeral) ?? true) // And isn't ephemeral
            )
            {
                DiscordMessageBuilder messageBuilder = paginator.Cancel();
                await paginator.CurrentMessage.ModifyAsync(messageBuilder);
                return true;
            }

            return false;
        }

        private async Task StartExpiringPaginatorsAsync()
        {
            while (await CleanupTimer.WaitForNextTickAsync())
            {
                await Parallel.ForEachAsync(CurrentPaginators.Values, async (paginator, cancellationToken) =>
                {
                    // 30 second timeout.
                    if (paginator != null && paginator.LastUpdatedAt.Add(PaginatorTimeout) <= DateTimeOffset.UtcNow)
                    {
                        if (paginator.CurrentMessage != null)
                        {
                            try
                            {
                                paginator.CurrentMessage = paginator.CurrentMessage!.Flags?.HasMessageFlag(MessageFlags.Ephemeral) ?? false
                                    ? await paginator.Interaction!.GetOriginalResponseAsync()
                                    : await paginator.CurrentMessage.Channel.GetMessageAsync(paginator.CurrentMessage.Id);
                            }
                            catch (DiscordException) { }
                        }
                        await RemovePaginatorAsync(paginator.Id, true);
                    }
                });
            }
        }

        [DiscordEvent]
        public static async Task PaginateAsync(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {
            PaginatorService paginatorService = client.GetCommandAllExtension()!.ServiceProvider.GetRequiredService<PaginatorService>();
            string? componentId = eventArgs.Message.Components.FirstOrDefault()?.Components.FirstOrDefault()?.CustomId;
            if (componentId == null || !Guid.TryParse(componentId.Split(':')[0], out Guid id))
            {
                return;
            }

            Paginator? paginator = paginatorService.GetPaginator(id);
            if (paginator == null)
            {
                return;
            }
            else if (paginator.Author.Id != eventArgs.User.Id)
            {
                paginator = paginatorService.CreatePaginator(paginator, eventArgs.User, paginator.CurrentMessage);
                paginator.Interaction = eventArgs.Interaction;
                DiscordMessageBuilder? response = HandlePagination(paginator, eventArgs, paginatorService);
                if (response != null)
                {
                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder(response).AsEphemeral());
                }
            }
            else
            {
                paginator.CurrentMessage = eventArgs.Message;
                paginator.Interaction = eventArgs.Interaction;
                DiscordMessageBuilder? response = HandlePagination(paginator, eventArgs, paginatorService);
                if (response != null)
                {
                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new(response));
                }
            }
        }

        [DiscordEvent]
        private static DiscordMessageBuilder? HandlePagination(Paginator paginator, ComponentInteractionCreateEventArgs eventArgs, PaginatorService paginatorService)
        {
            if (eventArgs.Values.Length != 0)
            {
                string instruction = eventArgs.Values[0].Split(':').Skip(1).First();
                return instruction switch
                {
                    "select-next" => paginator.GotoPage(paginator.GetNextSection()),
                    "select-previous" => paginator.GotoPage(paginator.GetPreviousSection()),
                    _ when int.TryParse(instruction, NumberStyles.Number, CultureInfo.InvariantCulture, out int pageNumber) => paginator.GotoPage(pageNumber),
                    _ => null
                };
            }
            else
            {
                string instruction = eventArgs.Interaction.Data.CustomId.Split(':')[1];
                // If the instruction is cancel and the message is either invoked by the user OR ephemeral, cancel the paginator.
                if (instruction == "cancel" && (eventArgs.User.Id == eventArgs.Message.Reference?.Message.Author.Id || eventArgs.Message.Interaction?.User.Id == eventArgs.User.Id))
                {
                    DiscordMessageBuilder response = paginator.Cancel();
                    paginatorService.RemovePaginatorAsync(paginator.Id, false).GetAwaiter().GetResult();
                    return response;
                }

                return instruction switch
                {
                    "first" => paginator.FirstPage(),
                    "last" => paginator.LastPage(),
                    "next" => paginator.NextPage(),
                    "previous" => paginator.PreviousPage(),
                    _ => null,
                };
            }
        }
    }
}
