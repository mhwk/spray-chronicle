using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Autofac;
using Shouldly;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.MessageHandling;
using Xunit;

namespace SprayChronicle.Persistence.Ouro.Test
{
    public class OuroEventStoreTest : OuroTestCase
    {
        [Fact]
        public void ItCanInstantiateOuroStore()
        {
            Container().Resolve<OuroEventStore>().ShouldNotBeNull();
        }
        
        [Fact]
        public void ItCanNotSaveEmptyStreamName()
        {
            var store = Container().Resolve<OuroEventStore>();
            Should.Throw<InvalidStreamException>(async () => await store.Append<Basket>("", new[] {
                new DomainMessage(0, new DateTime(), new object())
            }));
        }
        
        [Fact]
        public void ItCanNotSaveInvalidStreamName()
        {
            var store = Container().Resolve<OuroEventStore>();
            Should.Throw<InvalidStreamException>(async () => await store.Append<Basket>("@", new[] {
                new DomainMessage(0, new DateTime(), new object())
            }));
        }

        [Fact]
        public async Task LoadAppended()
        {
            var identity = Guid.NewGuid().ToString();
            var store = Container().Resolve<OuroEventStore>();
            var strategy = new OverloadMessagingStrategy<Basket>();
            var result = new List<object>();
            
            await store.Append<Basket>(identity, new [] {
                new DomainMessage(
                    0,
                    DateTime.Now,
                    new BasketPickedUp(identity)
                )
            });

            var source = store.Load<Basket>(identity);
            var convert = new TransformBlock<object,DomainMessage>(message => source.Convert(strategy, message));
            var action = new ActionBlock<DomainMessage>(message => result.Add(message.Payload));

            source.LinkTo(convert, new DataflowLinkOptions{
                PropagateCompletion = true
            });
            convert.LinkTo(action, new DataflowLinkOptions{
                PropagateCompletion = true
            });

            await Task.WhenAny(
                source.Start(),
                action.Completion
            );

            result.Count.ShouldBe(1);
            result.First().ShouldBeOfType<BasketPickedUp>();
            ((BasketPickedUp)result.First()).BasketId.ShouldBe(identity);
        }

        protected override void Configure(ContainerBuilder builder)
        {
            
        }
    }
}