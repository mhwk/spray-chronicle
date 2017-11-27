using System;
using SprayChronicle.Testing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Domain;
using Xunit;

namespace SprayChronicle.Example.Test.Domain
{
    public sealed class ItCanPickUpABasket : EventSourcedTestCase<Module>
    {
        protected override object When()
        {
            return new PickUpBasket("basketId");
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
