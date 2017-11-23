using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example;

namespace SprayChronicle.Testing.Test
{
    public class EventSourcedTestCaseTest : EventSourcedTestCase<Module>
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
