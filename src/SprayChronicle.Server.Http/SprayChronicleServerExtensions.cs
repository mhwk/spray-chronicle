using Autofac;
using SprayChronicle.CommandHandling;
using SprayChronicle.Projecting;
using SprayChronicle.QueryHandling;

namespace SprayChronicle.Server.Http
{
    public static class SprayChronicleServerExtensions
    {
        public static SprayChronicleServer WithHttp(this SprayChronicleServer server)
        {
            var http = new HttpServer();

            server.OnConfigure += builder => builder.RegisterModule<CommandHandlingModule>();
            server.OnConfigure += builder => builder.RegisterModule<ProjectingModule>();
            server.OnConfigure += builder => builder.RegisterModule<QueryHandlingModule>();
            server.OnConfigure += builder => builder.RegisterModule<SprayChronicleHttpModule>();
            server.OnInitialize += () => http.Initialize();
            server.OnExecute += container => http.Run();
            
            return server;
        }
    }
}
