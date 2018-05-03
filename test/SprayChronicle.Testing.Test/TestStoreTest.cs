using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SprayChronicle.EventHandling;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain.Model;
using Xunit;

namespace SprayChronicle.Testing.Test
{
    public class TestStoreTest
    {
        private readonly IEventStore _child = Substitute.For<IEventStore>();

        [Fact]
        public void RecordsThePast()
        {
            var message1 = new object();
            var message2 = new object();
            var store = new TestStore(_child, new EpochGenerator());
            
            store.Append<Basket>("basket1", new [] {
                new EventEnvelope(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    0,
                    message1,
                    DateTime.Now 
                ),
            });
            store.Present();
            store.Append<Basket>("basket1", new [] {
                new EventEnvelope(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    1,
                    message2,
                    DateTime.Now
                ),
            });
            
            store.Past().Select(p => p.Message).ShouldBe(new [] { message1 });
        }
        
        [Fact]
        public void RecordsTheFuture()
        {
            var message1 = new object();
            var message2 = new object();
            var store = new TestStore(_child, new EpochGenerator());
            
            store.Append<Basket>("basket1", new [] {
                new EventEnvelope(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    0,
                    message1,
                    DateTime.Now 
                ),
            });
            store.Present();
            store.Append<Basket>("basket1", new [] {
                new EventEnvelope(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    1,
                    message2,
                    DateTime.Now
                ),
            });
            
            store.Future().Select(p => p.Message).ShouldBe(new [] { message2 });
        }
        
        [Fact]
        public void RecordsTheChronicle()
        {
            var message1 = new object();
            var message2 = new object();
            var store = new TestStore(_child, new EpochGenerator());
            
            store.Append<Basket>("basket1", new [] {
                new EventEnvelope(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    0,
                    message1,
                    DateTime.Now
                ),
            });
            store.Present();
            store.Append<Basket>("basket1", new [] {
                new EventEnvelope(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    1,
                    message2,
                    DateTime.Now
                ),
            });
            
            store.Chronicle().Select(p => p.Message).ShouldBe(new [] { message1, message2 });
        }
        
        [Fact]
        public async Task LoadFromChild()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new object());
            
            var store = new TestStore(_child, new EpochGenerator());
            
            _child.Load<Basket>("basket1", "idempotencyId").Returns(source);
            store.Load<Basket>("basket1", "idempotencyId").ShouldBe(source);
        }
        
        [Fact]
        public void AppendToChild()
        {
            var epoch = DateTime.Now;
            var generator = new EpochGenerator();
            var message1 = new object();
            var store = new TestStore(_child, generator);
            var stream = new IEventEnvelope[] {
                new EventEnvelope(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    0,
                    message1,
                    epoch
                ),
            };
            
            generator.Add(epoch);
            
            store.Append<Basket>("basket1", stream);
            
            _child
                .Received()
                .Append<Basket>("basket1", Arg.Do<IEventEnvelope[]>(arg => arg.ShouldBeDeepEqualTo(stream)));
        }
    }
}
