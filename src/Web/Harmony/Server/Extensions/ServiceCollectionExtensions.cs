using Harmony.Application.Configurations;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Persistence;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Account;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Infrastructure.Seed;
using Harmony.Infrastructure.Services;
using Harmony.Infrastructure.Services.Identity;
using Harmony.Infrastructure.Services.Management;
using Harmony.Messaging;
using Harmony.Persistence.DbContext;
using Harmony.Persistence.Identity;
using Harmony.Server.Services;
using Harmony.Shared.Constants.Application;
using Harmony.Shared.Constants.Permission;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Harmony.Server.Extensions
{
    /// <summary>
    /// Extension class to register services per category
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        internal static AppConfiguration GetApplicationSettings(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            var applicationSettingsConfiguration = configuration.GetSection(nameof(AppConfiguration));
            services.Configure<AppConfiguration>(applicationSettingsConfiguration);
            return applicationSettingsConfiguration.Get<AppConfiguration>();
        }

        internal static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IRoleClaimService, RoleClaimService>();
            services.AddScoped<ITokenService, IdentityService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUploadService, UploadService>();
			services.AddScoped<ICardService, CardService>();
            services.AddScoped<IListService, ListService>();
            services.AddScoped<ICardActivityService, CardActivityService>();
            services.AddScoped<IBoardService, BoardService>();
            services.AddScoped<IMemberSearchService, MemberSearchService>();

			return services;
        }

        internal static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BrokerConfiguration>(configuration.GetSection("BrokerConfiguration"));

            services.AddSingleton<INotificationsPublisher, RabbitMQNotificationPublisher>();

            return services;
        }

        internal static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services
                .AddIdentity<HarmonyUser, HarmonyRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<HarmonyContext>()
                .AddDefaultTokenProviders();

            return services;
        }

        internal static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services, AppConfiguration config)
        {
            var key = Encoding.UTF8.GetBytes(config.Secret);
            services
                .AddAuthentication(authentication =>
                {
                    authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(async bearer =>
                {
                    bearer.RequireHttpsMetadata = false;
                    bearer.SaveToken = true;
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RoleClaimType = ClaimTypes.Role,
                        ClockSkew = TimeSpan.Zero
                    };

                    bearer.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments(ApplicationConstants.SignalR.HubUrl)))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = c =>
                        {
                            if (c.Exception is SecurityTokenExpiredException)
                            {
                                c.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                c.Response.ContentType = "application/json";
                                var result = JsonSerializer.Serialize(Result.Fail("The Token is expired."));
                                return c.Response.WriteAsync(result);
                            }
                            else
                            {
#if DEBUG
                                c.NoResult();
                                c.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                c.Response.ContentType = "text/plain";
                                return c.Response.WriteAsync(c.Exception.ToString());
#else
                                c.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                c.Response.ContentType = "application/json";
                                var result = JsonSerializer.Serialize(Result.Fail("An unhandled error has occurred."));
                                return c.Response.WriteAsync(result);
#endif
                            }
                        },
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            if (!context.Response.HasStarted)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.Response.ContentType = "application/json";
                                var result = JsonSerializer.Serialize(Result.Fail("You are not Authorized."));
                                return context.Response.WriteAsync(result);
                            }

                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            context.Response.ContentType = "application/json";
                            var result = JsonSerializer.Serialize(Result.Fail("You are not authorized to access this resource."));
                            return context.Response.WriteAsync(result);
                        },
                    };
                });

            services.AddAuthorization(options =>
            {
                // Here I stored necessary permissions/roles in a constant
                foreach (var prop in typeof(Permissions).GetNestedTypes().SelectMany(c => c.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
                {
                    var propertyValue = prop.GetValue(null);
                    if (propertyValue is not null)
                    {
                        options.AddPolicy(propertyValue.ToString(), policy => policy.RequireClaim(ApplicationClaimTypes.Permission, propertyValue.ToString()));
                    }
                }
            });
            return services;
        }

        internal static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
            => services
                .AddDbContext<HarmonyContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                    options.LogTo(s => System.Diagnostics.Debug.WriteLine(s));
                    options.EnableDetailedErrors(true);
                    options.EnableSensitiveDataLogging(true);
                })
                .AddScoped<IDatabaseSeeder, DatabaseRolesSeeder>()
                .AddScoped<IDatabaseSeeder, DatabaseUsersSeeder>()
                .AddScoped<IDatabaseSeeder, DatabaseWorkspaceSeeder>();

        internal static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            services
                .AddIdentity<HarmonyUser, HarmonyRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<HarmonyContext>()
                .AddDefaultTokenProviders();

            return services;
        }

        internal static IServiceCollection AddCurrentUserService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            return services;
        }

        internal static IServiceCollection AddClientNotificationService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IHubClientNotifierService, HubClientNotifierService>();
            return services;
        }

        internal static void RegisterSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(async c =>
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!assembly.IsDynamic)
                    {
                        var xmlFile = $"{assembly.GetName().Name}.xml";
                        var xmlPath = Path.Combine(baseDirectory, xmlFile);
                        if (File.Exists(xmlPath))
                        {
                            c.IncludeXmlComments(xmlPath);
                        }
                    }
                }

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Harmony"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API"//localizer["Input your Bearer token in this format - Bearer {your token here} to access this API"],
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    },
                });
            });
        }
    }
}
