using System;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Health;
using EventStore.ClientAPI;

namespace SprayChronicle.Persistence.Ouro
{
    public sealed class OuroHealthCheck : HealthCheck, IDisposable
    {
        private readonly IEventStoreConnection _connection;
        private bool _healthy;
        private string _state = "";
        private string _reason = "";
        
        public OuroHealthCheck(IEventStoreConnection connection) : base("EventStore")
        {
            _connection = connection;
            _connection.AuthenticationFailed += AuthenticationFailed;
            _connection.Closed += Closed;
            _connection.Connected += Connected;
            _connection.Disconnected += Disconnected;
            _connection.ErrorOccurred += ErrorOccurred;
            _connection.Reconnecting += Reconnecting;
        }

        public void Dispose()
        {
            _connection.AuthenticationFailed -= AuthenticationFailed;
            _connection.Closed -= Closed;
            _connection.Connected -= Connected;
            _connection.Disconnected -= Disconnected;
            _connection.ErrorOccurred -= ErrorOccurred;
            _connection.Reconnecting -= Reconnecting;
        }

        private void AuthenticationFailed(object sender, ClientAuthenticationFailedEventArgs e)
        {
            _healthy = false;
            _state = "Authentication failed";
            _reason = e.Reason;
        }

        private void Closed(object sender, ClientClosedEventArgs e)
        {
            _healthy = false;
            _state = "Closed";
            _reason = e.Reason;
        }

        private void Connected(object sender, ClientConnectionEventArgs e)
        {
            _healthy = true;
            _state = "Connected";
            _reason = "";
        }

        private void Disconnected(object sender, ClientConnectionEventArgs e)
        {
            _healthy = false;
            _state = "Disconnected";
        }

        private void ErrorOccurred(object sender, ClientErrorEventArgs e)
        {
            _healthy = false;
            _state = "Error occurred";
            _reason = e.Exception.ToString();
        }

        private void Reconnecting(object sender, ClientReconnectingEventArgs e)
        {
            _healthy = false;
            _state = "Reconnecting";
        }

        protected override async ValueTask<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = default (CancellationToken))
        {
            return _healthy
                ? HealthCheckResult.Healthy($"[{_state}]")
                : HealthCheckResult.Unhealthy($"[{_state}] {_reason}");
        }
    }
}