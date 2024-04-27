using Harmony.Application.Constants;
using Harmony.Application.Contracts.Repositories;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Harmony.Integrations.SourceControl.HealthChecks
{
    public class MongoDbHealthCheck : IHealthCheck
    {
        private readonly ISourceControlRepository _sourceControlRepository;

        public MongoDbHealthCheck(ISourceControlRepository sourceControlRepository)
        {
            _sourceControlRepository = sourceControlRepository;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionIsValid = await _sourceControlRepository.Ping(MongoDbConstants.SourceControlDatabase);
                
                if (connectionIsValid)
                {
                    return HealthCheckResult.Healthy("MongoDB connection established.");
                }

                return HealthCheckResult.Unhealthy("MongoDB connection pending.");
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy("MongoDB connection pending.");
            }
        }
    }
}
