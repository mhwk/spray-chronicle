namespace SprayChronicle.Server.Http
{
    public static class SprayChronicleServerExtensions
    {
        public static SprayChronicleServer WithHttp(this SprayChronicleServer server)
        {
            var http = new HttpServer();

            server.OnInitialize += () => http.Initialize();
            server.OnExecute += container => http.Run();
            
            return server;
        }
    }
}
