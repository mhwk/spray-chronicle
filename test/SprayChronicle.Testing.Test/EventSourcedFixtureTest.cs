using Xunit;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example;

namespace SprayChronicle.Testing.Test
{
    public class EventSourcedFixtureTest
    {
        public EventSourcedFixture<ExampleModule> Fixture()
        {
            return new EventSourcedFixture<ExampleModule>();
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
