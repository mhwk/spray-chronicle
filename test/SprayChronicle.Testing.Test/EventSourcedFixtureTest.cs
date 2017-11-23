using Xunit;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example;

namespace SprayChronicle.Testing.Test
{
    public class EventSourcedFixtureTest
    {
        private EventSourcedFixture<Module> Fixture()
        {
            return new EventSourcedFixture<Module>();
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
