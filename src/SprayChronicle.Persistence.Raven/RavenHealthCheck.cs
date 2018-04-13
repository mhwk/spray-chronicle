using System;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Health;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;

namespace SprayChronicle.Persistence.Raven
{
    public sealed class RavenHealthCheck : HealthCheck
    {
        private readonly IDocumentStore _store;
        
        public RavenHealthCheck(IDocumentStore store) : base("RavenDB")
        {
            _store = store;
        }
        
        protected override async ValueTask<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = default (CancellationToken))
        {
            try {
                var stats = await _store.Maintenance.SendAsync(new GetStatisticsOperation(), cancellationToken);
                return HealthCheckResult.Healthy($"[Connected] {stats.SizeOnDisk}MB");
            } catch (Exception error) {
                return HealthCheckResult.Unhealthy($"[Errored] {error}");
            }
        }
    }
}