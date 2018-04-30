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
                new DomainEnvelope(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    0,
                    new object(),
                    new DateTime()
                )
            }));
        }
        
        [Fact]
        public void ItCanNotSaveInvalidStreamName()
        {
            var store = Container().Resolve<OuroEventStore>();
            Should.Throw<InvalidStreamException>(async () => await store.Append<Basket>("@", new[] {
                new DomainEnvelope(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    0,
                    new object(),
                    new DateTime()
                )
            }));
        }

        [Fact]
        public async Task LoadAppended()
        {
            var identity = Guid.NewGuid().ToString();
            var store = Container().Resolve<OuroEventStore>();
            var strategy = new OverloadMailStrategy<Basket>();
            var result = new List<object>();
            
            await store.Append<Basket>(identity, new [] {
                new DomainEnvelope(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    0,
                    new BasketPickedUp(identity),
                    DateTime.Now
                )
            });

            var source = store.Load<Basket>(identity, "idempotencyId");
            var convert = new TransformBlock<object,DomainEnvelope>(message => source.Convert(strategy, message));
            var action = new ActionBlock<DomainEnvelope>(message => result.Add(message.Message));

            source.LinkTo(convert, new DataflowLinkOptions{
                PropagateCompletion = true
            });
            convert.LinkTo(action, new DataflowLinkOptions{
                PropagateCompletion = true
            });

            await source.Start();
            await source.Completion;
            await convert.Completion;
            await action.Completion;

            result.Count.ShouldBe(1);
            result.First().ShouldBeOfType<BasketPickedUp>();
            ((BasketPickedUp)result.First()).BasketId.ShouldBe(identity);
        }

        [Fact]
        public async Task ListProjections()
        {
            var projections = await Container().Resolve<OuroEventStore>().ListProjections();

            projections.ShouldContain("$by_category");
            projections.ShouldContain("$by_event_type");
            projections.ShouldContain("$stream_by_category");
            projections.ShouldContain("$streams");
        }

        [Fact]
        public async Task CreateProjection()
        {
            var projectionName = Guid.NewGuid().ToString();
            var store = Container().Resolve<OuroEventStore>();

            await store.EnsureProject(new StreamOptions(projectionName).From("Foo").BuildProjectionQuery());

            (await store.ListProjections()).ShouldContain(projectionName);
        }

        [Fact]
        public async Task RemoveProjection()
        {
            var projectionName = Guid.NewGuid().ToString();
            var store = Container().Resolve<OuroEventStore>();

            await store.EnsureProject(new StreamOptions(projectionName).From("Foo").BuildProjectionQuery());
            await store.DeleteProjection(new StreamOptions(projectionName).From("Foo").BuildProjectionQuery());

            (await store.ListProjections()).ShouldContain(projectionName);
        }



        protected override void Configure(ContainerBuilder builder)
        {
            
        }
    }
}