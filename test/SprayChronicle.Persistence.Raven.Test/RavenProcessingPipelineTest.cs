using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Autofac;
using Raven.Client.Documents;
using Shouldly;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Application.State;
using SprayChronicle.Example.Domain;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Persistence.Raven.Test
{
    public class RavenProcessingPipelineTest : RavenTestCase
    {
        [Fact]
        public async Task TestSavesDocument()
        {
            var identity1 = Guid.NewGuid().ToString();
            
            var store = Container()
                .Resolve<IDocumentStore>();
            var pipeline = Container()
                .Resolve<RavenProcessingPipeline<QueryBasketWithProducts, BasketWithProducts_v1>>();

            var source = (TestSource<QueryBasketWithProducts>) Container().Resolve<IEventSourceFactory>().Build<QueryBasketWithProducts,CatchUpOptions>(new CatchUpOptions("foo"));
            await source.Publish(new BasketPickedUp(identity1));

            await Task.WhenAny(
                pipeline.Start(),
                Task.Delay(TimeSpan.FromSeconds(.5))
            );

            using (var session = store.OpenAsyncSession()) {
                var result = await session.LoadAsync<BasketWithProducts_v1>(identity1);
                result.ShouldNotBeNull();
            }
        }

        protected override void Configure(ContainerBuilder builder)
        {
            builder.RegisterQueryExecutor<QueryBasketWithProducts,BasketWithProducts_v1>("foo");
        }
    }
}
