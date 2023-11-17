using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Events;

namespace OoLunar.Tomoe.Services.Pagination
{
    /// <summary>
    /// Edits a <see cref="DiscordMessage"/> or <see cref="DiscordInteraction"/> to display a paginated list of <see cref="Page"/>s.
    /// </summary>
    public sealed class PaginatorService
    {
        /// <summary>
        /// A dictionary of all the current paginators.
        /// </summary>
        private ConcurrentDictionary<Guid, Paginator> CurrentPaginators { get; init; } = new();

        /// <summary>
        /// A timer that runs every second to check if any paginators have expired.
        /// </summary>
        private PeriodicTimer CleanupTimer { get; init; } = new(TimeSpan.FromSeconds(1));

        /// <summary>
        /// The amount of time before a paginator expires.
        /// </summary>
        private TimeSpan PaginatorTimeout { get; init; }

        /// <summary>
        /// Creates a new <see cref="PaginatorService"/>.
        /// </summary>
        /// <param name="configuration">The configuration used for setting the timeout.</param>
        public PaginatorService(IConfiguration configuration)
        {
            PaginatorTimeout = configuration.GetValue("discord:paginator_timeout", TimeSpan.FromSeconds(30));
            _ = StartExpiringPaginatorsAsync();
        }

        /// <summary>
        /// Returns a new paginator with the given pages and author.
        /// </summary>
        /// <param name="pages">The pages to iterate through.</param>
        /// <param name="author">The user who's in control.</param>
        /// <returns>A new paginator.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pages"/> or <paramref name="author"/> is null.</exception>
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

        /// <summary>
        /// Creates a new paginator from an existing paginator.
        /// </summary>
        /// <param name="existingPaginator">The existing paginator to copy from.</param>
        /// <param name="newAuthor">The new user who's in control.</param>
        /// <param name="newMessage">The new message to attach the paginator too.</param>
        /// <returns>The new paginator.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="existingPaginator"/> or <paramref name="newAuthor"/> is null.</exception>
        public Paginator CreatePaginator(Paginator existingPaginator, DiscordUser newAuthor, DiscordMessage? newMessage = null)
        {
            ArgumentNullException.ThrowIfNull(existingPaginator, nameof(existingPaginator));
            ArgumentNullException.ThrowIfNull(newAuthor, nameof(newAuthor));

            Guid id = Guid.NewGuid();
            while (CurrentPaginators.ContainsKey(id))
            {
                id = Guid.NewGuid();
            }

            Paginator paginator = new(id, existingPaginator, newAuthor) { CurrentMessage = newMessage };
            CurrentPaginators.AddOrUpdate(paginator.Id, paginator, (key, value) => paginator);
            return paginator;
        }

        /// <summary>
        /// Retrieves a paginator from the current paginators. Returns <see langword="null"/> if the paginator doesn't exist.
        /// </summary>
        /// <param name="paginatorId">The paginator's id.</param>
        /// <returns>A <see cref="Paginator"/> if it's found, otherwise <see langword="null"/>.</returns>
        public Paginator? GetPaginator(Guid paginatorId) => CurrentPaginators.TryGetValue(paginatorId, out Paginator? paginator) ? paginator : null;

        /// <summary>
        /// Removes a paginator from the current paginators.
        /// </summary>
        /// <param name="paginatorId">The paginator to remove.</param>
        /// <param name="editMessage">Whether to edit the message to disable it or not.</param>
        /// <returns>A task representing whether the paginator was modified and successfully removed.</returns>
        public async Task<bool> RemovePaginatorAsync(Guid paginatorId, bool editMessage = false)
        {
            if (CurrentPaginators.TryRemove(paginatorId, out Paginator? paginator) // If the paginator exists
                && paginator != null // And isn't null
                && editMessage // And we're supposed to edit the message
                && paginator.CurrentMessage is not null // And has sent a message
                && (!paginator.CurrentMessage.Flags?.HasFlag(MessageFlags.Ephemeral) ?? true) // And isn't ephemeral
            )
            {
                DiscordMessageBuilder messageBuilder = paginator.Cancel();
                await paginator.CurrentMessage.ModifyAsync(messageBuilder);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Starts the timer that expires paginators.
        /// </summary>
        private async Task StartExpiringPaginatorsAsync()
        {
            while (await CleanupTimer.WaitForNextTickAsync())
            {
                await Parallel.ForEachAsync(CurrentPaginators.Values, async (paginator, cancellationToken) =>
                {
                    // 30 second timeout.
                    if (paginator != null && paginator.LastUpdatedAt.Add(PaginatorTimeout) <= DateTimeOffset.UtcNow)
                    {
                        if (paginator.CurrentMessage is not null)
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

        /// <summary>
        /// Handles the pagination of a message.
        /// </summary>
        /// <param name="client">The DiscordClient attached for that shard.</param>
        /// <param name="eventArgs">The event args containing information on who pressed what button.</param>
        [DiscordEvent]
        public static async Task PaginateAsync(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {
            PaginatorService paginatorService = client.GetCommandsExtension()!.ServiceProvider.GetRequiredService<PaginatorService>();
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

        /// <summary>
        /// Returns a <see cref="DiscordMessageBuilder"/> based on the paginator's current state.
        /// </summary>
        /// <param name="paginator">The paginator to get the next page from.</param>
        /// <param name="eventArgs">The event args containing which button/drop down the user had pressed.</param>
        /// <param name="paginatorService">The paginator service attached to the interaction.</param>
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
