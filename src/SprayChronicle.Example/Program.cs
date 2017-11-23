using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Autofac.Extensions.DependencyInjection;
using SprayChronicle.EventHandling;
using SprayChronicle.Persistence.Mongo;
using SprayChronicle.Persistence.Ouro;
using SprayChronicle.Server;
using SprayChronicle.Server.Http;

namespace SprayChronicle.Example
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            new ChronicleServer()
                .WithEventHandling()
                .WithHttp()
//                .WithOuroPersistence()
//                .WithMongoPersistence()
                .WithModule<Module>()
                .Run();
        }
    }
}
