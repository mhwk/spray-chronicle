using System;
using System.Threading.Tasks;
using Autofac;
using Raven.Client.Documents;
using Shouldly;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.QueryHandling;
using Xunit;

namespace SprayChronicle.Persistence.Raven.Test
{
    public class RavenExecutionPipelineTest : RavenTestCase
    {
        [Fact]
        public async Task ExecuteFind()
        {
            var identity1 = Guid.NewGuid().ToString();
            
            var store = Container()
                .Resolve<IDocumentStore>();
            var pipeline = Container()
                .Resolve<RavenExecutionPipeline<QueryBasketWithProducts, QueryBasketWithProducts.BasketWithProducts_v1>>();
            var router = new QueryRouter();
            var dispatcher = new RouterQueryDispatcher(router);
            router.Subscribe(pipeline);
            
            using (var session = store.OpenAsyncSession()) {
                await session.StoreAsync(
                    new QueryBasketWithProducts.BasketWithProducts_v1(
                        identity1,
                        DateTime.Now),
                    $"{typeof(QueryBasketWithProducts.BasketWithProducts_v1).Name}/{identity1}"
                );
                await session.SaveChangesAsync();
            }

            var task = pipeline.Start();

            var result = await dispatcher.Dispatch(new BasketById(identity1));

            await pipeline.Stop();
            await task;
            
            result.ShouldBeOfType<QueryBasketWithProducts.BasketWithProducts_v1>();
        }

        protected override void Configure(ContainerBuilder builder)
        {
            builder.RegisterQueryExecutor<QueryBasketWithProducts,QueryBasketWithProducts.BasketWithProducts_v1>("foo");
        }
    }
}
