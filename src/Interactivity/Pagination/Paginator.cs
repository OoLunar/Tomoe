using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Interactivity.Pagination
{
    public sealed class Paginator
    {
        public DiscordUser Author { get; init; }
        public DiscordMessage? CurrentMessage { get; set; }
        public DiscordInteraction? Interaction { get; set; }
        public Page[] Pages { get; init; }
        public int CurrentPage { get; private set; }
        public DateTimeOffset LastUpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public Ulid Id { get; init; }
        public bool IsCancelled => CurrentPage == -1;

        public Paginator(Ulid id, IEnumerable<Page> pages, DiscordUser author)
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

        public Paginator(Ulid newId, Paginator existingPaginator, DiscordUser newAuthor)
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
                return new DiscordMessageBuilder()
                    .WithContent(CurrentMessage!.Content)
                    .AddEmbed(CurrentMessage.Embeds[0])
                    .WithAllowedMentions(Mentions.None)
                    .AddComponents(CurrentMessage.FilterComponents<DiscordButtonComponent>().Select(button => button.Disable()))
                    .AddComponents(CurrentMessage.FilterComponents<DiscordSelectComponent>().Select(select => select.Disable()));
            }

            List<DiscordSelectComponentOption> options = new();
            int pageSelection = Math.Min(23, Pages.Length - CurrentPage);
            int endIndex = CurrentPage + pageSelection;
            for (int i = CurrentPage; i < endIndex; i++)
            {
                options.Add(new DiscordSelectComponentOption($"Page {i + 1:N0}: {Pages[i].Title?.Split('.').Last()}".Truncate(100), $"{Id}-{i.ToString(CultureInfo.InvariantCulture)}", Pages[i].Description.Truncate(100), i == CurrentPage, Pages[i].Emoji is not null ? new(Pages[i].Emoji!) : null!));
            }

            if (CurrentPage != 0)
            {
                options = options.Prepend(new DiscordSelectComponentOption("Previous Page Selection", $"{Id}-select-previous", "Shows the last 23 pages available.", false, new("⏪"))).ToList();
            }

            if (Pages.Length - CurrentPage > 23)
            {
                options.Add(new DiscordSelectComponentOption("Next Page Selection", $"{Id}-select-next", $"Shows the next {Pages.Length - 23} pages available.", false, new("⏩")));
            }

            return new DiscordMessageBuilder()
                .WithContent(Pages[CurrentPage].Content!)
                .AddEmbed(Pages[CurrentPage].Embed!)
                .WithAllowedMentions(Mentions.None)
                .AddComponents(new DiscordComponent[] {
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"{Id}-first", null!, false, new("⏪")),
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"{Id}-previous", null!, false, new("◀")),
                    new DiscordButtonComponent(DiscordButtonStyle.Danger, $"{Id}-cancel", null!, false, new("✖️")),
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"{Id}-next", null!, false, new("▶")),
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, $"{Id}-last", null!, false, new("⏩"))
                })
            .AddComponents(new DiscordSelectComponent("select", "Navigate Pages...", options));
        }
    }
}
