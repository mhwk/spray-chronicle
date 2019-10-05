using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Shouldly;
using Xunit;

namespace SprayChronicle.Mongo.Test
{
    public class MongoProjectorTest
    {
//        private readonly TestServer _server;

        private TestServer CreateServer()
        {
            return new TestServer(new WebHostBuilder()
                .UseConfiguration(new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddInMemoryCollection(new[] {
                        new KeyValuePair<string, string>("Mongo:Database",
                            "Test-" + DateTime.Now.ToString("yyyyMMdd-hhmmss-ffffff")),
                    })
                    .Build())
                .Configure((host, config) => {})
                .ConfigureServices(services => {
                    ((MongoServiceBuilder) services.AddMongo())
                        .DisableHostedServices()
                        .AddProjector<TestProjector>(2);
                }));
        }

        [Fact]
        public async Task ShouldIncrement()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            using var server = CreateServer();
            
            var events = server.Services.GetRequiredService<IStoreEvents>();
            await events.Append<Counter>(new[] {
                new Envelope<object>("1", "Counter", 0, new Increment {Id = "1"}),
                new Envelope<object>("1", "Counter", 1, new Increment {Id = "1"}),
                new Envelope<object>("2", "Counter", 1, new Increment {Id = "2"}),
                new Envelope<object>("2", "Counter", 2, new Increment {Id = "2"}),
            });


            var collection = server.Services
                .GetRequiredService<IMongoDatabase>()
                .GetCollection<Counter>(typeof(Counter).Name);
            await collection.InsertOneAsync(
                new Counter {Id = "2", Value = 1},
                new InsertOneOptions(),
                cancellationTokenSource.Token
            );

            var projector = server.Services.GetRequiredService<MongoProjector<TestProjector>>();
            await projector.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(500);

            collection.AsQueryable().Where(c => c.Id == "1").First().Value.ShouldBe(2);
            collection.AsQueryable().Where(c => c.Id == "2").First().Value.ShouldBe(3);
        }

        public class TestProjector : IProject
        {
            public async Task<Projection> Project(Envelope<object> envelope)
            {
                switch (envelope.Message) {
                    default: return new Projection.None();
                    case Increment e:
                        return new Projection.Mutate<Counter>(
                            e.Id,
                            s => new Counter {Id = e.Id, Value = (s?.Value ?? 0) + 1}
                        );
                }
            }
        }

        public class Increment
        {
            public string Id { get; set; }
        }

        public class Counter
        {
            [BsonId] public string Id { get; set; }
            public int Value { get; set; }
        }
    }
}
