using SprayChronicle.Server.Http;

namespace SprayChronicle.Example.Application
{
    [HttpQuery("basket/{basketId}")]
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
