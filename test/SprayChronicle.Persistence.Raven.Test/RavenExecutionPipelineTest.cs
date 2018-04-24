﻿using System;
using System.Threading.Tasks;
using Autofac;
using Raven.Client.Documents;
using Shouldly;
using SprayChronicle.EventSourcing;
using SprayChronicle.Example.Application;
using SprayChronicle.Example.Application.Service;
using SprayChronicle.Example.Application.State;
using SprayChronicle.Example.Domain;
using SprayChronicle.QueryHandling;
using SprayChronicle.Testing;
using Xunit;

namespace SprayChronicle.Persistence.Raven.Test
{
    public class RavenExecutionPipelineTest : RavenTestCase
    {
        [Fact]
        public async Task ExecuteAQuery()
        {
            var identity1 = Guid.NewGuid().ToString();
            
            var store = Container()
                .Resolve<IDocumentStore>();
            var pipeline = Container()
                .Resolve<RavenExecutionPipeline<QueryBasketWithProducts, BasketWithProducts_v1>>();
            var router = new QueryRouter();
            router.Subscribe(pipeline);
            
            using (var session = store.OpenAsyncSession()) {
                await session.StoreAsync(new BasketWithProducts_v1(identity1, DateTime.Now));
                await session.SaveChangesAsync();
            }

            var task = pipeline.Start();

            var result = await router.Route(new BasketById(identity1));

            await Task.WhenAll(
//                task,
                pipeline.Stop()
            );
            
            result.ShouldBeOfType<BasketWithProducts_v1>();
        }

        protected override void Configure(ContainerBuilder builder)
        {
            builder.RegisterQueryExecutor<QueryBasketWithProducts,BasketWithProducts_v1>("foo");
        }
    }
}