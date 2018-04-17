using System;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using System.Linq;
using System.Threading.Tasks;
using SprayChronicle.EventHandling;
using Processed = SprayChronicle.EventHandling.Processed;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class HandleOrder : CommandHandler<HandleOrder,Order>,
        IHandle<GenerateOrder>,
        IProcess<BasketCheckedOut>
    {
        public async Task<Handled> Handle(GenerateOrder command)
        {
            return await Handle()
                .Mutate(() => Order.Generate(
                    command.OrderId,
                    command.ProductIds.Cast<ProductId>().ToArray()
                ));
        }
        
        public async Task<Processed> Process(BasketCheckedOut message, DateTime epoch)
        {
            return await Process()
                .Dispatch(new GenerateOrder(
                    message.OrderId,
                    message.ProductIds
                ));
        }
    }
}
