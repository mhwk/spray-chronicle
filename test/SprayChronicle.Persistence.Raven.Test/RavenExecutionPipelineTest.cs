using System;
using System.Threading.Tasks;
using Autofac;
using Raven.Client.Documents;
using Shouldly;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Application.State;
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
                .Resolve<RavenExecutionPipeline<QueryBasketWithProducts, BasketWithProducts_v6>>();
            var router = new QueryRouter();
            router.Subscribe(pipeline);
            
            using (var session = store.OpenAsyncSession()) {
                await session.StoreAsync(new BasketWithProducts_v6(identity1, DateTime.Now));
                await session.SaveChangesAsync();
            }

            var task = pipeline.Start();

            var result = await router.Route(new BasketById(identity1));

            await Task.WhenAll(
//                task,
                pipeline.Stop()
            );
            
            result.ShouldBeOfType<BasketWithProducts_v6>();
        }

        protected override void Configure(ContainerBuilder builder)
        {
            builder.RegisterQueryExecutor<QueryBasketWithProducts,BasketWithProducts_v6>("foo");
        }
    }
}
