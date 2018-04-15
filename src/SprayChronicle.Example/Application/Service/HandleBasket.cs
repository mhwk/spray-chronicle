using System.Threading.Tasks;
using SprayChronicle.CommandHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain.Model;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class HandleBasket : CommandHandler<HandleBasket, Basket>,
        IHandle<PickUpBasket>,
        IHandle<AddProductToBasket>,
        IHandle<RemoveProductFromBasket>,
        IHandle<CheckOutBasket>
    {
        public async Task<CommandHandled> Handle(PickUpBasket command)
        {
            return await Handle(command.BasketId)
                .Mutate(() => Basket.PickUp(command.BasketId));
        }

        public async Task<CommandHandled> Handle(AddProductToBasket command)
        {
            return await Handle<PickedUpBasket>(command.BasketId)
                .Mutate(basket => basket.AddProduct(command.ProductId));
        }

        public async Task<CommandHandled> Handle(RemoveProductFromBasket command)
        {
            return await Handle<PickedUpBasket>(command.BasketId)
                .Mutate(basket => basket.RemoveProduct(command.ProductId));
        }

        public async Task<CommandHandled> Handle(CheckOutBasket command)
        {
            return await Handle<PickedUpBasket>(command.BasketId)
                .Mutate(basket => basket.CheckOut(command.OrderId));
        }
    }
}
