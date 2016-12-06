using Xunit;
using FluentAssertions;
using SprayChronicle.Example.Contracts.Events;

namespace SprayChronicle.EventHandling.Test
{
    public class NamespaceTypeLocatorTest
    {
        [Fact]
        public void ItFindsTypes()
        {
            var locator = new NamespaceTypeLocator("SprayChronicle.Example.Contracts.Events");
            locator.Locate("BasketPickedUp").ShouldBeEquivalentTo(typeof(BasketPickedUp));
        }
    }
}