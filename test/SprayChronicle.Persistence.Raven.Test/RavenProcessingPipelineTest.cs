using System;
using System.Threading.Tasks;
using Autofac;
using Raven.Client.Documents;
using Shouldly;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Domain;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Persistence.Raven.Test
{
    public class RavenProcessingPipelineTest : RavenTestCase
    {
        private readonly string _checkpointName = Guid.NewGuid().ToString();

        [Fact]
        public async Task TestSavesNewDocument()
        {
            var identity1 = Guid.NewGuid().ToString();
            
            var store = Container()
                .Resolve<IDocumentStore>();
            var pipeline = Container()
                .Resolve<RavenProcessingPipeline<QueryBasketWithProducts, QueryBasketWithProducts.BasketWithProducts_v1>>();
            var source = (TestSource<QueryBasketWithProducts>) Container().Resolve<IEventSourceFactory>().Build<QueryBasketWithProducts,CatchUpOptions>(new CatchUpOptions("foo"));
            var task = pipeline.Start();
            
            await source.Publish(new BasketPickedUp(identity1));
            source.Complete();

            await task;

            using (var session = store.OpenAsyncSession()) {
                var result = await session.LoadAsync<QueryBasketWithProducts.BasketWithProducts_v1>(
                    $"{typeof(QueryBasketWithProducts.BasketWithProducts_v1).Name}/{identity1}"
                );
                result.ShouldNotBeNull();
            }
        }
        
        [Fact]
        public async Task TestUpdatesPreviousDocument()
        {
            var identity1 = Guid.NewGuid().ToString();
            
            var store = Container()
                .Resolve<IDocumentStore>();
            var pipeline = Container()
                .Resolve<RavenProcessingPipeline<QueryBasketWithProducts, QueryBasketWithProducts.BasketWithProducts_v1>>();
            var source = (TestSource<QueryBasketWithProducts>) Container().Resolve<IEventSourceFactory>().Build<QueryBasketWithProducts,CatchUpOptions>(new CatchUpOptions("foo"));
            var task = pipeline.Start();
            
            await source.Publish(new BasketPickedUp(identity1));
            await source.Publish(new ProductAddedToBasket(identity1, "productId"));
            source.Complete();

            await task;
            
            using (var session = store.OpenAsyncSession()) {
                var result = await session.LoadAsync<QueryBasketWithProducts.BasketWithProducts_v1>($"{typeof(QueryBasketWithProducts.BasketWithProducts_v1).Name}/{identity1}");
                result.ShouldNotBeNull();
                result.ProductIds.Count.ShouldBe(1);
            }

            source.Complete();
        }

        protected override void Configure(ContainerBuilder builder)
        {
            builder.RegisterQueryExecutor<QueryBasketWithProducts,QueryBasketWithProducts.BasketWithProducts_v1>("foo", _checkpointName);
        }
    }
}
