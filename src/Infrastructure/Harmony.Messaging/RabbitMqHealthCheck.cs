using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Harmony.Messaging
{
    public class RabbitMqHealthCheck : IHealthCheck
    {
        private volatile bool _isConnected;

        public bool Connected
        {
            get => _isConnected;
            set => _isConnected = value;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (Connected)
            {
                return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ connection established."));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ connection pending."));
        }
    }
}
