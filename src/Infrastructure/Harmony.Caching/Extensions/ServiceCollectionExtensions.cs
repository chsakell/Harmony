using EasyCaching.Core.Configurations;
using EasyCaching.InMemory;
using EasyCaching.Serialization.SystemTextJson.Configurations;
using Google.Protobuf.WellKnownTypes;
using Harmony.Application.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Harmony.Caching.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
        {
            var cacheProvider = configuration["CacheProvider"];

            if (cacheProvider == null || !System.Enum
                .TryParse<CacheProvider>(cacheProvider, out var provider))
            {
                return services.UseInMemoryCaching();
            }

            return provider switch
            {
                CacheProvider.InMemory => services.UseInMemoryCaching(),
                CacheProvider.Redis => services.UseRedisCaching(configuration),
                _ => services.UseInMemoryCaching(),
            };
        }

        private static IServiceCollection UseInMemoryCaching(this IServiceCollection services)
        {
            //Important step for In-Memory Caching
            services.AddEasyCaching(options =>
            {
                // use memory cache with your own configuration
                options.UseInMemory(config =>
                {
                    config.DBConfig = new InMemoryCachingOptions
                    {
                        // scan time, default value is 60s
                        ExpirationScanFrequency = 60,
                        // total count of cache items, default value is 10000
                        SizeLimit = 1000,

                        // below two settings are added in v0.8.0
                        // enable deep clone when reading object from cache or not, default value is true.
                        EnableReadDeepClone = true,
                        // enable deep clone when writing object to cache or not, default value is false.
                        EnableWriteDeepClone = true,
                    };
                    // the max random second will be added to cache's expiration, default value is 120
                    config.MaxRdSecond = 120;
                    // whether enable logging, default is false
                    config.EnableLogging = false;
                    // mutex key's alive time(ms), default is 5000
                    config.LockMs = 5000;
                    // when mutex key alive, it will sleep some time, default is 300
                    config.SleepMs = 300;
                });
            });

            return services.RegisterInMemoryCache();
        }

        private static IServiceCollection UseRedisCaching(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionInfo = configuration["RedisConnectionString"].Split(":");
            var host = redisConnectionInfo[0];
            var port = int.Parse(redisConnectionInfo[1]);

            services.AddEasyCaching(option =>
            {
                option.WithSystemTextJson((options) =>
                {
                    options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                }, "json");

                option.UseRedis(config =>
                {
                    config.DBConfig.Endpoints.Add(new ServerEndPoint(host, port));
                }, "json");
            });

            return services.RegisterRedisCache();
        }

        private static IServiceCollection RegisterInMemoryCache(this IServiceCollection services)
        {
            services.AddSingleton<ICacheService, InMemoryCacheService>();

            return services;
        }

        private static IServiceCollection RegisterRedisCache(this IServiceCollection services)
        {
            services.AddSingleton<ICacheService, RedisCacheService>();

            return services;
        }
    }
}
