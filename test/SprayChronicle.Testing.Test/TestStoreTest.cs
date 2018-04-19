using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
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
                new DomainMessage(
                    0, DateTime.Now, message1 
                ),
            });
            store.Present();
            store.Append<Basket>("basket1", new [] {
                new DomainMessage(
                    1, DateTime.Now, message2
                ),
            });
            
            store.Past().Select(p => p.Payload).ShouldBe(new [] { message1 });
        }
        
        [Fact]
        public void RecordsTheFuture()
        {
            var message1 = new object();
            var message2 = new object();
            var store = new TestStore(_child, new EpochGenerator());
            
            store.Append<Basket>("basket1", new [] {
                new DomainMessage(
                    0, DateTime.Now, message1 
                ),
            });
            store.Present();
            store.Append<Basket>("basket1", new [] {
                new DomainMessage(
                    1, DateTime.Now, message2
                ),
            });
            
            store.Future().Select(p => p.Payload).ShouldBe(new [] { message2 });
        }
        
        [Fact]
        public void RecordsTheChronicle()
        {
            var message1 = new object();
            var message2 = new object();
            var store = new TestStore(_child, new EpochGenerator());
            
            store.Append<Basket>("basket1", new [] {
                new DomainMessage(
                    0, DateTime.Now, message1 
                ),
            });
            store.Present();
            store.Append<Basket>("basket1", new [] {
                new DomainMessage(
                    1, DateTime.Now, message2
                ),
            });
            
            store.Chronicle().Select(p => p.Payload).ShouldBe(new [] { message1, message2 });
        }
        
        [Fact]
        public async Task LoadFromChild()
        {
            var source = new TestSource<Basket>();
            await source.Publish(new object());
            
            var store = new TestStore(_child, new EpochGenerator());
            
            _child.Load<Basket>("basket1").Returns(source);
            store.Load<Basket>("basket1").ShouldBe(source);
        }
        
        [Fact]
        public void AppendToChild()
        {
            var epoch = DateTime.Now;
            var generator = new EpochGenerator();
            var message1 = new object();
            var store = new TestStore(_child, generator);
            var stream = new IDomainMessage[] {
                new DomainMessage(
                    0, epoch, message1
                ),
            };
            
            generator.Add(epoch);
            
            store.Append<Basket>("basket1", stream);
            
            _child
                .Received()
                .Append<Basket>("basket1", Arg.Do<IDomainMessage[]>(arg => arg.ShouldBeDeepEqualTo(stream)));
        }
    }
}
