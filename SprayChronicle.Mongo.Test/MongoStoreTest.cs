using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SprayChronicle.Test;
using SprayChronicle.Test.Example.Domain;
using SprayChronicle.Test.Example.Domain.Model;
using Xunit;

namespace SprayChronicle.Mongo.Test
{
    public class MongoStoreTest
    {
        private readonly TestServer _server;

        public MongoStoreTest()
        {
            _server = new TestServer(new WebHostBuilder()
                .ConfigureAppConfiguration(builder => {
                    builder.AddEnvironmentVariables();
                })
                .Configure((builder, configure) => {
                })
                .ConfigureServices(services => {
                    services.AddMongo();
                }));            
        }
        
        [Fact]
        public async Task ShouldLoadAppendedEnvelopes()
        {
            var invariantId = Guid.NewGuid().ToString();
            var events = _server.Services.GetRequiredService<IStoreEvents>();
            
            await events.Append<Shopping>(new [] {
                new Envelope<object>(
                    invariantId,
                    typeof(Shopping).Name,
                    0,
                    new ProductChosen(
                        "customerId",
                        "productId"
                    )
                ), 
            });

            var envelopes = await events
                .Load<Shopping>(invariantId, null, -1)
                .ToSync();
            
            envelopes.Length.ShouldBe(1);
            envelopes[0].Message.ShouldBeOfType<ProductChosen>();
            ((ProductChosen)envelopes[0].Message).CustomerId.ShouldBe("customerId");
            ((ProductChosen)envelopes[0].Message).ProductId.ShouldBe("productId");
        }
        
        [Fact]
        public async Task ShouldNotLoadEnvelopesMatchingCausationId()
        {
            var invariantId = Guid.NewGuid().ToString();
            var causationId = Guid.NewGuid().ToString();
            var events = _server.Services.GetRequiredService<IStoreEvents>();
            
            await events.Append<Shopping>(new [] {
                new Envelope<object>(
                    invariantId,
                    typeof(Shopping).Name,
                    0,
                    new ProductChosen(
                        "customerId",
                        "productId"
                    )
                ).CausedBy(causationId), 
            });

            await events
                .Load<Shopping>(invariantId, causationId, -1)
                .ToSync()
                .ShouldThrowAsync<IdempotencyException>();
        }

        [Fact]
        public async Task ShouldLoadInitialSnapshot()
        {
            var invariantId = Guid.NewGuid().ToString();
            var snapshots = _server.Services.GetRequiredService<IStoreSnapshots>();
            var snapshot = await snapshots.Load<Shopping>(invariantId, null);
            
            snapshot.Identity.ShouldBe(invariantId);
            snapshot.Sequence.ShouldBe(-1);
            snapshot.Snap.ShouldBeOfType<Shopping>();
        }

        [Fact]
        public async Task ShouldLoadSavedSnapshot()
        {
            var invariantId = Guid.NewGuid().ToString();
            var snapshots = _server.Services.GetRequiredService<IStoreSnapshots>();
            var snapshot1 = await snapshots.Load<Shopping>(invariantId, null);
            snapshot1.Sequence = 1;
            await snapshots.Save<Shopping>(snapshot1);
            
            var snapshot2 = await snapshots.Load<Shopping>(invariantId, null);
            
            snapshot2.Identity.ShouldBe(invariantId);
            snapshot2.Sequence.ShouldBe(1);
            snapshot2.Snap.ShouldBeOfType<Shopping>();
        }

        [Fact]
        public async Task ShouldNotLoadSnapshotWithSameCausationId()
        {
            var invariantId = Guid.NewGuid().ToString();
            var causationId = Guid.NewGuid().ToString();
            var events = _server.Services.GetRequiredService<IStoreEvents>();
            var snapshots = _server.Services.GetRequiredService<IStoreSnapshots>();
            
            await events.Append<Shopping>(new [] {
                new Envelope<object>(
                    invariantId,
                    typeof(Shopping).Name,
                    0,
                    new ProductChosen(
                        "customerId",
                        "productId"
                    )
                ).CausedBy(causationId), 
            });
            
            await snapshots
                .Load<Shopping>(invariantId, causationId)
                .ShouldThrowAsync<IdempotencyException>();
        }
    }
}
