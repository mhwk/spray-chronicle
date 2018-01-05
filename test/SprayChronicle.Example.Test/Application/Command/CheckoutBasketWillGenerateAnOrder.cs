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
        protected override Task Given(IDispatchCommands dispatcher)
        {
            return dispatcher.Dispatch(
                new PickUpBasket("basketId"),
                new AddProductToBasket("basketId", "productId")
            );
        }
        
        protected override Task When(IDispatchCommands dispatcher)
        {
            return dispatcher.Dispatch(
                new CheckOutBasket("basketId", "orderId")
            );
        }

        protected override void Then(IValidate validator)
        {
            validator.Expect(
                new BasketCheckedOut("basketId", "orderId", new [] {"productId"})
//                new OrderGenerated("orderId", new [] {"productId"})
            );
        }

        [Fact]
        public override async Task Scenario()
        {
            await base.Scenario();
        }
    }
}
