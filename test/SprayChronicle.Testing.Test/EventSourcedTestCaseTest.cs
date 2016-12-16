using SprayChronicle.Example.Contracts.Commands;
using SprayChronicle.Example.Contracts.Events;
using SprayChronicle.Example;

namespace SprayChronicle.Testing.Test
{
    public class EventSourcedTestCaseTest : EventSourcedTestCase<ExampleCoordinationModule>
    {
        protected override object[] Given()
        {
            return new object[] {
                new BasketPickedUp("foo")
            };
        }

        protected override object When()
        {
            return new AddProductToBasket("foo", "bar");
        }

        protected override object[] Expect()
        {
            return new object[] {
                new ProductAddedToBasket("foo", "bar")
            };
        }
    }
}
