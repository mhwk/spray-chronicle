using System;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public class ErrorSuppressingRouter : IRouter<IHandle>
    {
        private readonly IRouter<IHandle> _child;

        public ErrorSuppressingRouter(IRouter<IHandle> child)
        {
            _child = child;
        }

        public IRouter<IHandle> Subscribe(IMessageHandlingStrategy<IHandle> strategy, HandleMessage handler)
        {
            _child.Subscribe(strategy, handler);
            return this;
        }

        public IRouter<IHandle> Subscribe(IRouterSubscriber<IHandle> subscriber)
        {
            _child.Subscribe(subscriber);
            return this;
        }

        public async Task<object> Route(params object[] arguments)
        {
            try {
                await _child.Route(arguments);
            } catch (Exception) {
                // ignored
            }

            return null;
        }
    }
}