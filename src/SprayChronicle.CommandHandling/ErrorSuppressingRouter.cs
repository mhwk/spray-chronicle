using System;
using System.Threading.Tasks;
using SprayChronicle.MessageHandling;

namespace SprayChronicle.CommandHandling
{
    public class ErrorSuppressingRouter : IMessageRouter
    {
        private readonly IMessageRouter _child;

        public ErrorSuppressingRouter(IMessageRouter child)
        {
            _child = child;
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