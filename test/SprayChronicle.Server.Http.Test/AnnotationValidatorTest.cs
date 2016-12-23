using Xunit;
using SprayChronicle.Example.Contracts.Commands;

namespace SprayChronicle.Server.Http.Test
{
    public class AnnotationValidatorTest
    {
        [Fact]
        public void ItValidates()
        {
            Assert.Throws<InvalidatedException>(() => new AnnotationValidator().Validate(new PickUpBasket(null)));
        }
    }
}