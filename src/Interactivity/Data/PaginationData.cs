using System.Collections.Generic;
using OoLunar.Tomoe.Interactivity.Pagination;

namespace OoLunar.Tomoe.Interactivity.Data
{
    public sealed record PaginationData : IdleData
    {
        public required IReadOnlyList<Page> Pages { get; init; }
        public int CurrentIndex { get; set; }
    }
}
