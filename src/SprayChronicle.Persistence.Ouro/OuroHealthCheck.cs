using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Health;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroHealthCheck : HealthCheck
    {
        private readonly HttpClient _client = new HttpClient();
        
        public OuroHealthCheck(string name) : base(name)
        {
        }

        protected override async ValueTask<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = default (CancellationToken))
        {
            var response = await _client.GetAsync("http://eventstore/gossip", cancellationToken);

            return ! response.IsSuccessStatusCode
                ? HealthCheckResult.Unhealthy("Unable to reach eventstore")
                : HealthCheckResult.Healthy("Eventstore reachable");
        }
    }
}