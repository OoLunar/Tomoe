using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Services.Pagination
{
    public sealed class Paginator
    {
        public DiscordUser Author { get; init; }
        public DiscordMessage? CurrentMessage { get; set; }
        public DiscordInteraction? Interaction { get; set; }
        public Page[] Pages { get; init; }
        public int CurrentPage { get; private set; }
        public DateTimeOffset LastUpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public Guid Id { get; init; }
        public bool IsCancelled => CurrentPage == -1;

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

        public DiscordMessageBuilder NextPage()
        {
            CurrentPage = CurrentPage == Pages.Length - 1 ? 0 : CurrentPage + 1;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        public DiscordMessageBuilder PreviousPage()
        {
            CurrentPage = CurrentPage == 0 ? Pages.Length - 1 : CurrentPage - 1;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        public DiscordMessageBuilder FirstPage()
        {
            CurrentPage = 0;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        public DiscordMessageBuilder LastPage()
        {
            CurrentPage = Pages.Length - 1;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        public DiscordMessageBuilder Cancel()
        {
            CurrentPage = -1;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        public DiscordMessageBuilder GotoPage(int pageNumber)
        {
            CurrentPage = pageNumber;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return GenerateMessage();
        }

        public int GetPreviousSection()
        {
            CurrentPage = CurrentPage - 23 < 0 ? 0 : CurrentPage - 23;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return CurrentPage;
        }

        public int GetNextSection()
        {
            CurrentPage = CurrentPage + 23 > Pages.Length - 1 ? Pages.Length - 1 : CurrentPage + 23;
            LastUpdatedAt = DateTimeOffset.UtcNow;
            return CurrentPage;
        }

        public DiscordMessageBuilder GenerateMessage()
        {
            if (CurrentPage == -1)
            {
                DiscordMessageBuilder messageBuilder = new(CurrentMessage);
                messageBuilder.ClearComponents();
                return messageBuilder
                    .WithAllowedMentions(Mentions.None)
                    .AddComponents(CurrentMessage!.Components.First().Components.Cast<DiscordButtonComponent>().Select(button => button.Disable()))
                    .AddComponents(((DiscordSelectComponent)CurrentMessage.Components.ElementAt(1).Components.First()).Disable());
            }

            List<DiscordSelectComponentOption> options = new();
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
                options.Add(new DiscordSelectComponentOption($"Page {i + 1:N0}: {page.Title}".Truncate(100, "…"), $"{Id}:{i.ToString(CultureInfo.InvariantCulture)}", page.Description.Truncate(100), i == CurrentPage, page.Emoji != null ? new(page.Emoji) : null));
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
                    new DiscordButtonComponent(ButtonStyle.Secondary, $"{Id}:first", null, false, new("⏪")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, $"{Id}:previous", null, false, new("◀")),
                    new DiscordButtonComponent(ButtonStyle.Danger, $"{Id}:cancel", null, false, new("✖️")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, $"{Id}:next", null, false, new("▶")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, $"{Id}:last", null, false, new("⏩"))
                })
                .AddComponents(new DiscordSelectComponent("select", "Navigate Pages...", options));
        }
    }
}
