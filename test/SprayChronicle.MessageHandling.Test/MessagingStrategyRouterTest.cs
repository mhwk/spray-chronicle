using System.Threading.Tasks;
using Shouldly;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventHandling;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.MessageHandling.Test
{
    public class MessagingStrategyRouterTest
    {
        [Fact]
        public void WontRoute()
        {
            var router = new TestRouter();
            
            Should.Throw<UnroutableMessageException>(() => router.Route(new TestEnvelope(new PickUpBasket("basketId"))));
        }

        [Fact]
        public async Task DoesRoute()
        {
            var routed = false;
            var router = new TestRouter();
            
            router.Subscribe(new OverloadMailStrategy<HandleBasket>() as IMailStrategy, message => {
                routed = true;
                return Task.FromResult<object>(null);
            });

            await router.Route(new TestEnvelope(new PickUpBasket("basketId")));
            
            routed.ShouldBeTrue();
        }

        public class TestRouter : MailStrategyRouter<IHandle>
        {
        }
    }
}
