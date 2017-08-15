namespace SprayChronicle.Example.Contracts.Queries
{
    public sealed class PagedNumberOfProductsInBasket
    {
        public int Page { get; } = 1;

        public int PerPage { get; } = 1;

        public PagedNumberOfProductsInBasket(int page, int perPage)
        {
            Page = page;
            PerPage = perPage;
        }
    }
}