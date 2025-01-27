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

        public DiscordSelectComponent CreateDropdown(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages)
        {
            if (currentPageIndex == -1)
            {
                return new(
                    customId: $"{id}_dropdown",
                    placeholder: $"Select a page (Disabled)",
                    options: pages.Select((page, index) => new DiscordSelectComponentOption(
                        label: page.Title ?? $"Page {index + 1}",
                        value: index.ToString(CultureInfo.InvariantCulture),
                        description: page.Description!,
                        isDefault: false,
                        emoji: page.Emoji is not null ? new DiscordComponentEmoji(page.Emoji) : null!)
                    ).Take(25),
                    disabled: true
                );
            }

            int startingIndex = Math.Max(currentPageIndex - 12, 0);
            int maxIndex = startingIndex + CalculatePageCount(startingIndex, pages.Count);

            List<DiscordSelectComponentOption> options = [];
            for (int i = startingIndex; i < maxIndex; i++)
            {
                Page page = pages[i];
                options.Add(new DiscordSelectComponentOption(
                    label: page.Title ?? $"Page {i + 1}/{pages.Count}",
                    value: i.ToString(CultureInfo.InvariantCulture),
                    description: page.Description!,
                    isDefault: currentPageIndex == i,
                    emoji: page.Emoji is not null ? new DiscordComponentEmoji(page.Emoji) : null!)
                );
            }

            if ((maxIndex - startingIndex) < 25)
            {
                int section = currentPageIndex / 23;
                if (section > 0)
                {
                    int previousSectionIndex = Math.Max(0, ((section - 1) * 23) - 12);
                    int previousSectionMaxIndex = previousSectionIndex + CalculatePageCount(previousSectionIndex, pages.Count);
                    options.Insert(0, new DiscordSelectComponentOption(
                        label: "Previous Section",
                        value: $"{(section - 1) * 23}{char.MinValue}",
                        description: $"Go to the previous section (pages {previousSectionIndex + 1}-{previousSectionMaxIndex})",
                        isDefault: false,
                        emoji: new DiscordComponentEmoji("⬅️")
                    ));
                }

                if (maxIndex < pages.Count)
                {
                    int nextSectionIndex = Math.Max(1, ((section + 1) * 23) - 12);
                    int nextSectionMaxIndex = nextSectionIndex + CalculatePageCount(nextSectionIndex, pages.Count);
                    options.Add(new DiscordSelectComponentOption(
                        label: "Next Section",
                        value: $"{(section + 1) * 23}{char.MinValue}",
                        description: $"Go to the next section (pages {nextSectionIndex + 1}-{nextSectionMaxIndex})",
                        isDefault: false,
                        emoji: new DiscordComponentEmoji("➡️")
                    ));
                }
            }

            return new(
                customId: $"{id}_dropdown",
                placeholder: $"Select a page ({currentPageIndex + 1}/{pages.Count})",
                options: options
            );
        }

        private static int CalculatePageCount(int startingIndex, int totalPageCount)
        {
            if (startingIndex == 0)
            {
                // Return 24 so that the "Next Section" button is added.
                return totalPageCount > 25 ? 24 : 25;
            }

            // Return 23 so that the "Previous Section" and "Next Section" buttons are added.
            return totalPageCount - startingIndex > 24 ? 23 : totalPageCount - startingIndex;
        }
    }
}
