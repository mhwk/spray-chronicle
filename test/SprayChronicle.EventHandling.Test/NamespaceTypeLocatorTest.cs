using Xunit;
using FluentAssertions;
using SprayChronicle.Example.Domain;

namespace SprayChronicle.EventHandling.Test
{
    public class NamespaceTypeLocatorTest
    {
        [Fact]
        public void ItFindsTypes()
        {
            var locator = new NamespaceTypeLocator("SprayChronicle.Example.Domain");
            locator.Locate("BasketPickedUp").ShouldBeEquivalentTo(typeof(BasketPickedUp));
        }
    }
}