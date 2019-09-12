using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SprayChronicle.Mongo
{
    public class MongoOptionsConfigure : IConfigureOptions<MongoOptions>
    {
        private readonly IConfiguration _configuration;
        
        public MongoOptionsConfigure(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public void Configure(MongoOptions options)
        {
            Console.WriteLine("Binding configuration");
            _configuration.GetSection("Mongo").Bind(options);
            Console.WriteLine(options.ConnectionString);
        }
    }
}
