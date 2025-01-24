using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity.Moments.Pagination
{
    public class PaginationDefaultComponentCreator : IPaginationComponentCreator
    {
        public DiscordButtonComponent CreateFirstPageButton(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages)
            => new(DiscordButtonStyle.Primary, $"{id}_first", "First", currentPageIndex is 0 or -1, new DiscordComponentEmoji("⏮️"));

        public DiscordButtonComponent CreatePreviousPageButton(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages)
            => new(DiscordButtonStyle.Primary, $"{id}_previous", "Previous", currentPageIndex is 0 or -1, new DiscordComponentEmoji("◀️"));

        public DiscordButtonComponent CreateStopButton(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages)
            => new(DiscordButtonStyle.Secondary, $"{id}_stop", "Stop", currentPageIndex == -1, new DiscordComponentEmoji("⏹️"));

        public DiscordButtonComponent CreateNextPageButton(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages)
            => new(DiscordButtonStyle.Primary, $"{id}_next", "Next", currentPageIndex == -1 || currentPageIndex == (pages.Count - 1), new DiscordComponentEmoji("▶️"));

        public DiscordButtonComponent CreateLastPageButton(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages)
            => new(DiscordButtonStyle.Primary, $"{id}_last", "Last", currentPageIndex == -1 || currentPageIndex == (pages.Count - 1), new DiscordComponentEmoji("⏭️"));

        public DiscordSelectComponent CreateDropdown(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages) => new(
            $"{id}_dropdown",
            "Select a page",
            pages.Select((page, index) => new DiscordSelectComponentOption(
                page.Title ?? $"Page {index + 1}",
                index.ToString(CultureInfo.InvariantCulture),
                page.Description!,
                index == currentPageIndex,
                page.Emoji is not null ? new DiscordComponentEmoji(page.Emoji) : null!)
            ),
            currentPageIndex == -1
        );
    }
}
