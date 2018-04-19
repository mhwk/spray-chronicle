﻿using System.Threading.Tasks;
using SprayChronicle.CommandHandling;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Example.Test.Application.Command
{
    public class CheckoutBasketWillGenerateAnOrder : CommandTestCase<Module>
    {
        protected override Task Given(CommandRouter dispatcher)
        {
            return dispatcher.Route(
                new PickUpBasket("basketId"),
                new AddProductToBasket("basketId", "productId")
            );
        }
        
        protected override Task When(CommandRouter dispatcher)
        {
            return dispatcher.Route(
                new CheckOutBasket("basketId", "orderId")
            );
        }

        protected override void Then(IValidate validator)
        {
            validator.Expect(
                new BasketCheckedOut("basketId", "orderId", new [] {"productId"}),
                new OrderGenerated("orderId", new [] {"productId"})
            );
        }

        [Fact]
        public override async Task Scenario()
        {
            await base.Scenario();
        }
    }
}
