using System;
using SprayChronicle.Testing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using Xunit;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class PickUpABasket : EventSourcedTestCase<Module,Basket>
    {
        protected override Basket When(Basket basket)
        {
            return Basket.PickUp(new BasketId("basketId"));
        }

        protected override object[] Expect()
        {
            return new object[] {
                new BasketPickedUp("basketId")
            };
        }

        [Fact]
        public override void Scenario()
        {
            base.Scenario();
        }
    }
}
