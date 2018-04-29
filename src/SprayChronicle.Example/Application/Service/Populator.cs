using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SprayChronicle.CommandHandling;

namespace SprayChronicle.Example.Application.Service
{
    public sealed class Populator
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public Populator(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        public async Task Populate()
        {
            var random = new Random();
            var tasks = new List<Task>();
            
            for (var i = 0; i < 1000000; i++) {
                await Task.Delay(TimeSpan.FromMilliseconds(random.Next(0, 1000)));
                tasks.Add(Task.Run(async () => {
                    try {
                        var basketId = Guid.NewGuid().ToString();
                        
                        await Task.Delay(TimeSpan.FromMilliseconds(random.Next(1, 1000)));
                        
                        await _commandDispatcher.Dispatch(new PickUpBasket(basketId));

                        await Task.Delay(TimeSpan.FromMilliseconds(random.Next(1, 100)));
                        
                        for (var x = 0; x < random.Next(0, 10); x++) {
                            await _commandDispatcher.Dispatch(new AddProductToBasket(basketId, Guid.NewGuid().ToString()));
                            
                            await Task.Delay(TimeSpan.FromMilliseconds(random.Next(1, 1000)));
                        
                        }
                    
                        if (0 == random.Next(0, 4)) {
                            await _commandDispatcher.Dispatch(new CheckOutBasket(basketId, Guid.NewGuid().ToString()));
                        }
                    } catch (Exception error) {
                        Console.WriteLine($"Whoops: {error}");
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}
