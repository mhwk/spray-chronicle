using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SprayChronicle.Server.HealthChecks
{
    public class HealthCheckConsoleCommand : IConsoleCommand
    {
        public string Name => "health";
        
        public string Description => "Check health of running instance";

        public Func<Task<int>> Execute => async () =>
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://0.0.0.0/_health");

            return response.IsSuccessStatusCode ? 0 : 1;
        };
    }
}
