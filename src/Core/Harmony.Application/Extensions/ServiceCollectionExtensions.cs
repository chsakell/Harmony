using Microsoft.Extensions.DependencyInjection;
using Polly;
using System.Reflection;
using static Harmony.Shared.Constants.Application.ApplicationConstants;


namespace Harmony.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        }

        public static void AddRetryPolicies(this IServiceCollection services)
        {
            services.AddResiliencePipeline(HarmonyRetryPolicy.WaitAndRetry, builder =>
            {
                builder.AddRetry(new()
                {
                    ShouldHandle = new Polly.PredicateBuilder().Handle<Exception>(),
                    MaxRetryAttempts = 5,
                    Delay = TimeSpan.FromMilliseconds(500),
                    OnRetry = args =>
                    {
                        var exception = args.Outcome.Exception!;
                        return default;
                    }
                });
            });
        }
    }
}