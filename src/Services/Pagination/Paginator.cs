using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Services.Pagination
{
    /// <summary>
    /// Handles the turning of pages on a paginated <see cref="DiscordMessage"/>.
    /// </summary>
    public sealed class Paginator
    {
        /// <summary>
        /// Whom the paginator belongs to.
        /// </summary>
        public DiscordUser Author { get; init; }

        /// <summary>
        /// The current message being shown to the user.
        /// </summary>
        public DiscordMessage? CurrentMessage { get; set; }

        /// <summary>
        /// The current interaction being shown to the user.
        /// </summary>
        public DiscordInteraction? Interaction { get; set; }

        /// <summary>
        /// All pages available in the paginator.
        /// </summary>
        public Page[] Pages { get; init; }

        /// <summary>
        /// The index of the current page.
        /// </summary>
        public int CurrentPage { get; private set; }

        /// <summary>
        /// When the user had last interacted with the paginator. Used for timeouts.
        /// </summary>
        // TODO: Move this to the paginator service.
        public DateTimeOffset LastUpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// A unique identifier for the paginator.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Whether the current paginator has been cancelled, either by the user or by a timeout. This determines if the paginator is currently usable.
        /// </summary>
        public bool IsCancelled => CurrentPage == -1;

        /// <summary>
        /// Creates a new <see cref="Paginator"/>.
        /// </summary>
        /// <param name="id">The id to assign to the paginator.</param>
        /// <param name="pages">The pages that the paginator will contain.</param>
        /// <param name="author">Who controls the paginator.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="author"/> or <paramref name="pages"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="pages"/> has less than two pages.</exception>
        public Paginator(Guid id, IEnumerable<Page> pages, DiscordUser author)
        {
            ArgumentNullException.ThrowIfNull(author, nameof(author));
            ArgumentNullException.ThrowIfNull(pages, nameof(pages));
            if (pages.Count() < 2)
            {
                throw new ArgumentException("Paginator must have at least two pages.", nameof(pages));
            }

            Id = id;
            Author = author;
            Pages = pages.ToArray();
        }

        /// <summary>
        /// Creates a new <see cref="Paginator"/> from an existing one.
        /// </summary>
        /// <param name="newId">The id to assign to the new paginator.</param>
        /// <param name="existingPaginator">The existing paginator to copy from.</param>
        /// <param name="newAuthor">Who controls the new paginator.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="existingPaginator"/> or <paramref name="newAuthor"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="existingPaginator"/> is already cancelled.</exception>
        public Paginator(Guid newId, Paginator existingPaginator, DiscordUser newAuthor)
        {
            ArgumentNullException.ThrowIfNull(existingPaginator, nameof(existingPaginator));
            ArgumentNullException.ThrowIfNull(newAuthor, nameof(newAuthor));
            if (existingPaginator.IsCancelled)
            {
                throw new ArgumentException("Paginator is already cancelled.", nameof(existingPaginator));
            }

            Id = newId;
            Author = newAuthor;
            Pages = existingPaginator.Pages;
            CurrentPage = existingPaginator.CurrentPage;
        }

        /// <summary>
        /// Flips the paginator to the next page, updating the <see cref="CurrentPage"/> and <see cref="LastUpdatedAt"/> properties.
        /// </summary>
        public DiscordMessageBuilder NextPage()
        {
            CurrentPage = CurrentPage == Pages.Length - 1 ? 0 : CurrentPage + 1;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        /// <summary>
        /// Flips the paginator to the previous page, updating the <see cref="CurrentPage"/> and <see cref="LastUpdatedAt"/> properties.
        /// </summary>
        public DiscordMessageBuilder PreviousPage()
        {
            CurrentPage = CurrentPage == 0 ? Pages.Length - 1 : CurrentPage - 1;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        /// <summary>
        /// Flips the paginator to the first page, updating the <see cref="CurrentPage"/> and <see cref="LastUpdatedAt"/> properties.
        /// </summary>
        public DiscordMessageBuilder FirstPage()
        {
            CurrentPage = 0;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        /// <summary>
        /// Flips the paginator to the last page, updating the <see cref="CurrentPage"/> and <see cref="LastUpdatedAt"/> properties.
        /// </summary>
        public DiscordMessageBuilder LastPage()
        {
            CurrentPage = Pages.Length - 1;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        /// <summary>
        /// Cancels the paginator making it unusable.
        /// </summary>
        public DiscordMessageBuilder Cancel()
        {
            CurrentPage = -1;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        /// <summary>
        /// Flips the paginator to the specified page, updating the <see cref="CurrentPage"/> and <see cref="LastUpdatedAt"/> properties.
        /// </summary>
        public DiscordMessageBuilder GotoPage(int pageNumber)
        {
            CurrentPage = pageNumber;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        /// <summary>
        /// Returns the index to the previous section (23 pages).
        /// </summary>
        public int GetPreviousSection()
        {
            CurrentPage = CurrentPage - 23 < 0 ? 0 : CurrentPage - 23;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return CurrentPage;
        }

        /// <summary>
        /// Returns the index to the next section (23 pages).
        /// </summary>
        public int GetNextSection()
        {
            CurrentPage = CurrentPage + 23 > Pages.Length - 1 ? Pages.Length - 1 : CurrentPage + 23;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return CurrentPage;
        }

        /// <summary>
        /// Generates a <see cref="DiscordMessageBuilder"/> for the current page.
        /// </summary>
        /// <returns>A <see cref="DiscordMessageBuilder"/> for the current page.</returns>
        public DiscordMessageBuilder GenerateMessage()
        {
            if (CurrentPage == -1)
            {
                if (CurrentMessage is null)
                {
                    throw new InvalidOperationException("Cannot generate a new page for a paginator that hasn't been sent.");
                }

                DiscordMessageBuilder messageBuilder = new(CurrentMessage);
                messageBuilder.ClearComponents();
                return messageBuilder
                    .WithAllowedMentions(Mentions.None)
                    .AddComponents(CurrentMessage!.Components.First().Components.Cast<DiscordButtonComponent>().Select(button => button.Disable()))
                    .AddComponents(((DiscordSelectComponent)CurrentMessage.Components.ElementAt(1).Components.First()).Disable());
            }

            List<DiscordSelectComponentOption> options = [];
            int startIndex = Math.Min(23, Pages.Length - CurrentPage);
            int endIndex = CurrentPage + startIndex;
            if (startIndex == endIndex)
            {
                startIndex = Math.Max(CurrentPage - 23, 0);
                endIndex = Pages.Length;
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                Page page = Pages[i];
                options.Add(new DiscordSelectComponentOption($"Page {i + 1:N0}: {page.Title}".Truncate(100, "…"), $"{Id}:{i.ToString(CultureInfo.InvariantCulture)}", page.Description.Truncate(100), i == CurrentPage, page.Emoji is not null ? new(page.Emoji) : null!));
            }

            if (Pages.Length > 23)
            {
                if (CurrentPage != 0)
                {
                    options = options.Prepend(new DiscordSelectComponentOption("Previous Page Selection", $"{Id}:select-previous", "Shows the last 23 pages available.", false, new("⏪"))).ToList();
                }

                if (Pages.Length - CurrentPage > 23)
                {
                    options.Add(new DiscordSelectComponentOption("Next Page Selection", $"{Id}:select-next", $"Shows the next {Pages.Length - 23} pages available.", false, new("⏩")));
                }
            }

            return new DiscordMessageBuilder(Pages[CurrentPage].MessageBuilder)
                .WithAllowedMentions(Mentions.None)
                .AddComponents(new DiscordComponent[] {
                    new DiscordButtonComponent(ButtonStyle.Secondary, $"{Id}:first", null!, false, new("⏪")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, $"{Id}:previous", null!, false, new("◀")),
                    new DiscordButtonComponent(ButtonStyle.Danger, $"{Id}:cancel", null!, false, new("✖️")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, $"{Id}:next", null!, false, new("▶")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, $"{Id}:last", null!, false, new("⏩"))
                })
                .AddComponents(new DiscordSelectComponent("select", "Navigate Pages...", options));
        }
    }
}
