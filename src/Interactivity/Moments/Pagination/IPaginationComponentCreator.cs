using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.ComponentCreators;

namespace OoLunar.Tomoe.Interactivity.Moments.Pagination
{
    public interface IPaginationComponentCreator : IComponentCreator
    {
        public DiscordButtonComponent CreateFirstPageButton(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages);
        public DiscordButtonComponent CreatePreviousPageButton(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages);
        public DiscordButtonComponent CreateStopButton(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages);
        public DiscordButtonComponent CreateNextPageButton(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages);
        public DiscordButtonComponent CreateLastPageButton(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages);
        public DiscordSelectComponent CreateDropdown(Ulid id, int currentPageIndex, IReadOnlyList<Page> pages);
    }
}
