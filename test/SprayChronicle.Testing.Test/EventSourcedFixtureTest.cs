using Xunit;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Contracts.Commands;
using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.Testing.Test
{
    public class EventSourcedFixtureTest
    {
        public EventSourcedFixture<BasketHandler,Basket> Fixture()
        {
            return new EventSourcedFixture<BasketHandler,Basket>();
        }

        [Fact]
        public void ItCanTestInitialWhen()
        {
            Fixture()
                .When(new PickUpBasket("foo"))
                .ExpectNoException()
                .Expect(new BasketPickedUp("foo"));
        }

        [Fact]
        public void ItCanTestWhenAfterHistory()
        {
            Fixture()
                .Given(new BasketPickedUp("foo"))
                .When(new AddProductToBasket("foo", "bar"))
                .ExpectNoException()
                .Expect(new ProductAddedToBasket("foo", "bar"));
        }
    }
}
