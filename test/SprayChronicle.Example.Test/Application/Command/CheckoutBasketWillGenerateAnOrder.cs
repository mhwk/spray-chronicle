using System.Threading.Tasks;
using SprayChronicle.CommandHandling;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Example.Test.Application.Command
{
    public class CheckoutBasketWillGenerateAnOrder : CommandTestCase<Module>
    {
        protected override object[] Given()
        {
            return new object[] {
                new PickUpBasket("basketId"), 
                new AddProductToBasket("basketId", "productId"),
            };
        }
        
        protected override Task When(IDispatchCommand dispatcher)
        {
            return dispatcher.Dispatch(new CheckOutBasket(
                "basketId",
                "orderId"
            ));
        }

        protected override object[] Expect()
        {
            return new object[] {
                new BasketCheckedOut("basketId", "orderId", new [] {"productId"}),
                new OrderGenerated("orderId", new [] {"productId"}), 
            };
        }

        [Fact]
        public override void Scenario()
        {
            base.Scenario();
        }
    }
}
