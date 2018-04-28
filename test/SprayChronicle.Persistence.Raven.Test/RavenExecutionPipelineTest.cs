using System;
using System.Threading.Tasks;
using Autofac;
using Raven.Client.Documents;
using Shouldly;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Application.State;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;
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
                .Resolve<RavenExecutionPipeline<QueryBasketWithProducts, BasketWithProducts_v7>>();
            var router = new QueryRouter();
            var dispatcher = new RouterQueryDispatcher(router);
            router.Subscribe(pipeline);
            
            using (var session = store.OpenAsyncSession()) {
                await session.StoreAsync(new BasketWithProducts_v7(identity1, DateTime.Now), $"{typeof(BasketWithProducts_v7).Name}/{identity1}");
                await session.SaveChangesAsync();
            }

            var task = pipeline.Start();

            var result = await dispatcher.Dispatch(new BasketById(identity1));

            await Task.WhenAll(
//                task,
                pipeline.Stop()
            );
            
            result.ShouldBeOfType<BasketWithProducts_v7>();
        }

        protected override void Configure(ContainerBuilder builder)
        {
            builder.RegisterQueryExecutor<QueryBasketWithProducts,BasketWithProducts_v7>("foo");
        }
    }
}
