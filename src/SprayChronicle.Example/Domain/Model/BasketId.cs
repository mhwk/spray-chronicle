
namespace SprayChronicle.Example.Domain.Model
{
    public sealed class BasketId : Identifier
    {
        public BasketId(string id): base(id)
        {}

        public static implicit operator BasketId(string basketId)
        {
            return new BasketId(basketId);
        }
    }
}
