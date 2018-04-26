using System;
using System.Threading.Tasks;
using SprayChronicle.CommandHandling;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class Populator
    {
        private readonly CommandRouter _router;

        public Populator(CommandRouter router)
        {
            _router = router;
        }

        public async Task Populate()
        {
            var random = new Random();
            
            for (var i = 0; i < 10000; i++) {
                try {
                    var basketId = Guid.NewGuid().ToString();
                    await _router.Route(new PickUpBasket(basketId));
                    for (var x = 0; x < random.Next(0, 10); x++) {
                        await _router.Route(new AddProductToBasket(basketId, Guid.NewGuid().ToString()));
                    }
                    
                    if (0 == random.Next(0, 4)) {
                        await _router.Route(new CheckOutBasket(basketId, Guid.NewGuid().ToString()));
                    }
                } catch (Exception error) {
                    Console.WriteLine($"Whoops: {error}");
                }
            }
        }
    }
}
