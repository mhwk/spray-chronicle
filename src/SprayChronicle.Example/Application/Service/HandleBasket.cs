using System;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class HandleBasket : CommandHandler<Basket>
    {
        public HandleBasket(IEventSourcingRepository<Basket> repository): base(repository)
        {}
        
        private void Handle(PickUpBasket command)
        {
            Repository().Start(
                () => Basket.PickUp(new BasketId(command.BasketId))
            );
        }

        private void Handle(AddProductToBasket command)
        {
            Repository().Continue<PickedUpBasket>(
                command.BasketId,
                basket => basket.AddProduct(new ProductId(command.ProductId))
            );
        }

        private void Handle(RemoveProductFromBasket command)
        {
            Repository().Continue<PickedUpBasket>(
                command.BasketId,
                basket => basket.RemoveProduct(new ProductId(command.ProductId))
            );
        }

        private void Handle(CheckOutBasket command)
        {
            Repository().Continue<PickedUpBasket,CheckedOutBasket>(
                command.BasketId,
                basket => basket.CheckOut(new OrderId(command.OrderId))
            );
        }
    }
}
