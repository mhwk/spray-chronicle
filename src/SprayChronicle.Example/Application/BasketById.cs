namespace SprayChronicle.Example.Application
{
    public sealed class BasketById
    {
        public string BasketId { get; }
        
        public int Page { get; }

        public BasketById(string basketId, int page = 1)
        {
            BasketId = basketId;
            Page = page;
        }
    }
}
