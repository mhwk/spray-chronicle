using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class BasketHandler : OverloadCommandHandler<Basket>
    {
        public BasketHandler(IEventSourcingRepository<Basket> repository): base(repository)
        {}
        
        public void Handle(PickUpBasket command)
        {
            Start(
                () => Basket.PickUp(new BasketId(command.BasketId))
            );
        }

        public void Handle(AddProductToBasket command)
        {
            Continue<PickedUpBasket>(
                command.BasketId,
                basket => basket.AddProduct(new ProductId(command.ProductId))
            );
        }

        public void Handle(RemoveProductFromBasket command)
        {
            Continue<PickedUpBasket>(
                command.BasketId,
                basket => basket.RemoveProduct(new ProductId(command.ProductId))
            );
        }

        public void Handle(CheckOutBasket command)
        {
            Continue<PickedUpBasket,CheckedOutBasket>(
                command.BasketId,
                basket => basket.CheckOut(new OrderId(command.OrderId))
            );
        }
    }
}
