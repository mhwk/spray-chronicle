using System.Threading.Tasks;
using SprayChronicle.CommandHandling;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class HandleBasket : CommandHandler<HandleBasket, Basket>,
        IHandle<PickUpBasket>,
        IHandle<AddProductToBasket>,
        IHandle<RemoveProductFromBasket>,
        IHandle<CheckOutBasket>
    {
        public async Task<Handled> Handle(PickUpBasket command)
        {
            return await Handle(command.BasketId)
                .Mutate(() => Basket.PickUp(command.BasketId));
        }

        public async Task<Handled> Handle(AddProductToBasket command)
        {
            return await Handle<PickedUpBasket>(command.BasketId)
                .Mutate(basket => basket.AddProduct(command.ProductId));
        }

        public async Task<Handled> Handle(RemoveProductFromBasket command)
        {
            return await Handle<PickedUpBasket>(command.BasketId)
                .Mutate(basket => basket.RemoveProduct(command.ProductId));
        }

        public async Task<Handled> Handle(CheckOutBasket command)
        {
            return await Handle<PickedUpBasket>(command.BasketId)
                .Mutate(basket => basket.CheckOut(command.OrderId));
        }
    }
}
