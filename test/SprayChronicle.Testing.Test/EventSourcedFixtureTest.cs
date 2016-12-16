using Xunit;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Contracts.Commands;
using SprayChronicle.Example.Contracts.Events;
using SprayChronicle.Example;

namespace SprayChronicle.Testing.Test
{
    public class EventSourcedFixtureTest
    {
        public EventSourcedFixture<ExampleCoordinationModule> Fixture()
        {
            return new EventSourcedFixture<ExampleCoordinationModule>();
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
