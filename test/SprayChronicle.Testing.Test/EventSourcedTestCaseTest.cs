using Xunit;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Contracts.Commands;
using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.Testing.Test
{
    public class EventSourcedTestCaseTest : EventSourcedTestCase<BasketHandler,Basket>
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
