using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Internal;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class HandleBasket : CommandHandler<HandleBasket, Basket>
    {
        public HandleBasket(IEventSourcingRepository<Basket> repository): base(repository)
        {
        }
        
        private async Task Handle(PickUpBasket command)
        {
            await For(command.BasketId)
                .Mutate(() => Basket.PickUp(command.BasketId));
        }

        private async Task Handle(AddProductToBasket command)
        {
            await For<PickedUpBasket>(command.BasketId)
                .Mutate(basket => basket.AddProduct(command.ProductId));
        }

        private async Task Handle(RemoveProductFromBasket command)
        {
            await For<PickedUpBasket>(command.BasketId)
                .Mutate(basket => basket.RemoveProduct(command.ProductId));
        }

        private async Task Handle(CheckOutBasket command)
        {
            await For<PickedUpBasket>(command.BasketId)
                .Mutate(basket => basket.CheckOut(command.OrderId));
        }
    }
}
