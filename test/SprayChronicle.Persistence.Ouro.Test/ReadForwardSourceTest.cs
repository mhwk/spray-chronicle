using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Autofac;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using NSubstitute;
using Shouldly;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Domain.Model;
using SprayChronicle.MessageHandling;
using SprayChronicle.Server;
using Xunit;

namespace SprayChronicle.Persistence.Ouro.Test
{
    public class ReadForwardSourceTest : OuroTestCase
    {
        private ILogger<Basket> _logger = Substitute.For<ILogger<Basket>>();

        private IMessagingStrategy<Basket> _strategy = Substitute.For<IMessagingStrategy<Basket>>();
        
        [Fact]
        public async Task ReadsForwardFromEmptyStreamGracefully()
        {
            var streamName = Guid.NewGuid().ToString();
            var ouro = Container().Resolve<IEventStoreConnection>();
            var source = new ReadForwardSource<Basket>(
                _logger,
                ouro,
                new UserCredentials("admin", "changeit"),
                new ReadForwardOptions("SprayChronicle-" + streamName, 0)
            );
            
            var results = new List<DomainMessage>();
            var transform = new TransformBlock<object, DomainMessage>(resolved => source.Convert(_strategy, resolved));
            var action = new ActionBlock<DomainMessage>(domain => results.Add(domain));

            source.LinkTo(transform, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            transform.LinkTo(action, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await Task.WhenAny(
                source.Start(),
                action.Completion
//                Task.Delay(1000)
            );

            results.ShouldBeEmpty();
        }
        
        [Fact]
        public async Task ReadsForwardFromZero()
        {
            var streamName = Guid.NewGuid().ToString();
            var ouro = Container().Resolve<IEventStoreConnection>();
            var store = Container().Resolve<OuroEventStore>();
            var source = new ReadForwardSource<Basket>(
                _logger,
                ouro,
                new UserCredentials("admin", "changeit"),
                new ReadForwardOptions("SprayChronicle-" + streamName, 0)
            );

            await store.Append<Basket>(streamName, new [] { new DomainMessage(
                0,
                DateTime.Now,
                new TestMessage()
            )});

            var results = new List<DomainMessage>();
            var transform = new TransformBlock<object, DomainMessage>(resolved => source.Convert(_strategy, resolved));
            var action = new ActionBlock<DomainMessage>(domain => results.Add(domain));

            source.LinkTo(transform, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            transform.LinkTo(action, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            _strategy.Resolves(Arg.Is("TestMessage")).Returns(true);
            _strategy.ToType("TestMessage").Returns(typeof(TestMessage));
            
            await Task.WhenAny(
                source.Start(),
                action.Completion
//                Task.Delay(1000)
            );

            results.First().ShouldBeOfType<DomainMessage>();
        }
        
        [Fact]
        public async Task ReadsForwardFromCheckpoint()
        {
            var streamName = Guid.NewGuid().ToString();
            var ouro = Container().Resolve<IEventStoreConnection>();
            var store = Container().Resolve<OuroEventStore>();
            var source = new ReadForwardSource<Basket>(
                _logger,
                ouro,
                new UserCredentials("admin", "changeit"),
                new ReadForwardOptions("SprayChronicle-" + streamName, 1)
            );

            await store.Append<Basket>(streamName, new [] {
                new DomainMessage(
                    0,
                    DateTime.Now,
                    new TestMessage()
                ),
                new DomainMessage(
                    1,
                    DateTime.Now,
                    new TestMessage()
                ),
            });

            var results = new List<DomainMessage>();
            var transform = new TransformBlock<object, DomainMessage>(resolved => source.Convert(_strategy, resolved));
            var action = new ActionBlock<DomainMessage>(domain => results.Add(domain));

            source.LinkTo(transform, new DataflowLinkOptions {
                PropagateCompletion = true
            });
            transform.LinkTo(action, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            _strategy.Resolves(Arg.Is("TestMessage")).Returns(true);
            _strategy.ToType("TestMessage").Returns(typeof(TestMessage));
            
            await Task.WhenAny(
                source.Start(),
                action.Completion
//                Task.Delay(1000)
            );

            results.Count.ShouldBe(1);
        }

        protected override void Configure(ContainerBuilder builder)
        {
            
        }

        public class TestMessage
        {
        }
    }
}
