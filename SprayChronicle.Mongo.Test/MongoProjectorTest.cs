using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Shouldly;
using Xunit;

namespace SprayChronicle.Mongo.Test
{
    public class MongoProjectorTest
    {
        private readonly TestServer _server;

        public MongoProjectorTest()
        {
            _server = new TestServer(new WebHostBuilder()
                .UseConfiguration(new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddInMemoryCollection(new [] {
                        new KeyValuePair<string, string>("Mongo:Database", "Test-" + DateTime.Now.ToString("yyyyMMdd-hhmmss-ffffff")),
                    })
                    .Build())
                .Configure((host, config) => { })
                .ConfigureServices(services => {
                    services.AddMongo()
                        .AddProjector<TestProjector>(1);
                }));            
        }

        [Fact]
        public async Task ShouldIncrement()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            
            var events = _server.Services.GetRequiredService<IStoreEvents>();
            await events.Append<Counter>(new[] {
                new Envelope<object>("1", "Counter", 0, new Increment()),
                new Envelope<object>("1", "Counter", 1, new Increment()),
            });

            var projector = _server.Services
                .GetServices<IHostedService>()
                .First(p => p is MongoProjector<TestProjector>);
            
            await projector.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(500);
            await projector.StopAsync(cancellationTokenSource.Token);

            var collection = _server.Services
                .GetRequiredService<IMongoDatabase>()
                .GetCollection<Counter>(typeof(Counter).Name);
            
            collection.AsQueryable().Where(c => c.Id == "1").First().Value.ShouldBe(2);
        }

        public class TestProjector : IProject
        {
            public async Task<Projection> Project(Envelope<object> envelope)
            {
                switch (envelope.Message) {
                    default: return new Projection.None();
                    case Increment e: return new Projection.Mutate<Counter>(
                        "1",
                        s => new Counter {
                            Id = "1",
                            Value = (s?.Value ?? 0) + 1
                        }
                    );
                }
            }
        }

        public class Increment
        {
            
        }

        public class Counter
        {
            [BsonId]
            public string Id { get; set; }
            public int Value { get; set; }
        }
    }
}
