using Xunit;
using SprayChronicle.Example.Application;

namespace SprayChronicle.Server.Http.Test
{
    public class AnnotationValidatorTest
    {
        [Fact]
        public void ItValidates()
        {
            Assert.Throws<InvalidRequestException>(() => new AnnotationValidator().Validate(new PickUpBasket(null)));
        }
    }
}