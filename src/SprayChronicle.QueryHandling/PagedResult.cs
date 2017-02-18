using System.Collections.Generic;

namespace SprayChronicle.QueryHandling
{
    public sealed class PagedResult<T>
    {
            public readonly IEnumerable<T> Items;

            public readonly int Page;

            public readonly  int PerPage;

            public readonly int Total;

            public PagedResult(IEnumerable<T> items, int page, int perPage, int total)
            {
                Items = items;
                Page = page;
                PerPage = perPage;
                Total = total;
            }
    }
}