using System;
using System.Threading.Tasks;
using SprayChronicle.EventHandling;
using SprayChronicle.Example.Domain;
using SprayChronicle.Persistence.Raven;
using Processed = SprayChronicle.EventHandling.Processed;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class QueryPlacedOrders : RavenQueries<QueryPlacedOrders,QueryPlacedOrders.PlacedOrders_v3>,
        IProcess<BasketCheckedOut>,
        IProcess<OrderGenerated>
    {
        public async Task<Processed> Process(BasketCheckedOut payload, DateTime epoch)
        {
            return await Process(payload.OrderId)
                .Mutate(() => new PlacedOrders_v3(
                    payload.OrderId,
                    payload.ProductIds,
                    "checked_out",
                    epoch
                ));
        }

        public async Task<Processed> Process(OrderGenerated payload, DateTime epoch)
        {
            return await Process(payload.OrderId)
                .Mutate(order => order.WithStatus("invoiced"));
        }
        
        public sealed class PlacedOrders_v3
        {
            public string OrderId { get; }
            public string[] ProductIds { get; }
            public string Status { get; private set; }
            public DateTime CheckedOutAt { get; }

            public PlacedOrders_v3(string orderId, string[] productIds, string status, DateTime checkedOutAt)
            {
                OrderId = orderId;
                ProductIds = productIds;
                Status = status;
                CheckedOutAt = checkedOutAt;
            }

            public PlacedOrders_v3 WithStatus(string status)
            {
                Status = status;

                return this;
            }
        }
    }
}
