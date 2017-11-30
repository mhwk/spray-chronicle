using System.Threading.Tasks;
using SprayChronicle.CommandHandling;
using SprayChronicle.Example.Application;
using SprayChronicle.Testing;

namespace SprayChronicle.Example.Test.Application.Command
{
    public class CheckoutBasketWillGenerateAnOrder : CommandTestCase<Module>
    {
        protected override Task When(IDispatchCommand dispatcher)
        {
            return dispatcher.Dispatch(new CheckOutBasket(
                "basketId",
                "orderId"
            ));
        }
    }
}
