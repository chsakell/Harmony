using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Harmony.Messaging
{
    public class RabbitMqHealthCheck : IHealthCheck
    {
        /// <summary>How long a consumer holds back application startup waiting for its first broker connection.</summary>
        public static readonly TimeSpan StartupConnectionTimeout = TimeSpan.FromSeconds(15);

        private volatile bool _isConnected;

        // Completed the first time a consumer reports a connection
        private readonly TaskCompletionSource _firstConnection =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool Connected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;

                if (value)
                {
                    _firstConnection.TrySetResult();
                }
            }
        }

        /// <summary>
        /// Waits until a consumer has connected for the first time, or <paramref name="timeout"/> elapses.
        /// Consumers call it from StartAsync so the web server - and its /healthz/ready endpoint - only starts once the
        /// broker connection is in place, instead of answering the first readiness probes with "connection pending".
        /// </summary>
        /// <returns>true when connected, false on timeout.</returns>
        public async Task<bool> WaitForFirstConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            try
            {
                await _firstConnection.Task.WaitAsync(timeout, cancellationToken);
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
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
