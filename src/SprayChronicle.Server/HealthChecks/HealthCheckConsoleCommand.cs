using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SprayChronicle.Server.HealthChecks
{
    public class HealthCheckConsoleCommand : IConsoleCommand
    {
        public string Name => "health";
        
        public string Description => "Check health of local running instance";

        public Func<Task<int>> Execute => async () =>
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://127.0.0.1:5000/_health");

            Console.WriteLine(await response.Content.ReadAsStringAsync());

            return response.IsSuccessStatusCode ? 0 : 1;
        };
    }
}
