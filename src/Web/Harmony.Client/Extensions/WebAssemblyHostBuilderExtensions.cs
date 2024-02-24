using Blazored.LocalStorage;
using Harmony.Client.Infrastructure.Authentication;
using Harmony.Client.Infrastructure.Configuration;
using Harmony.Client.Infrastructure.Managers;
using Harmony.Client.Infrastructure.Managers.Preferences;
using Harmony.Client.Infrastructure.Store;
using Harmony.Shared.Constants.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using System.Globalization;
using System.Reflection;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Harmony.Client.Extensions
{
    public static class WebAssemblyHostBuilderExtensions
    {
        private const string ClientName = "Harmony.Client";
        public static WebAssemblyHostBuilder AddClientServices(this WebAssemblyHostBuilder builder, WebAssemblyHostConfiguration configuration)
        {
            var gatewayUrl = configuration["gatewayUrl"];

            var clientConfiguration = new ClientConfiguration(gatewayUrl);

            builder.Services.AddSingleton(clientConfiguration);

            builder
                .Services
                .AddLocalization(options =>
                {
                    options.ResourcesPath = "Resources";
                })
                .AddAuthorizationCore(options =>
                {
                    RegisterPermissionClaims(options);
                })
                .AddBlazoredLocalStorage()
                .AddMudServices(configuration =>
                {
                    configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
                    configuration.SnackbarConfiguration.HideTransitionDuration = 100;
                    configuration.SnackbarConfiguration.ShowTransitionDuration = 100;
                    configuration.SnackbarConfiguration.VisibleStateDuration = 3000;
                    configuration.SnackbarConfiguration.ShowCloseIcon = false;
                })
                .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())
                .AddScoped<ClientPreferenceManager>()
                .AddScoped<HarmonyStateProvider>()
                .AddScoped<AuthenticationStateProvider, HarmonyStateProvider>()
                .AddManagers()
                .AddStores()
                .AddScoped<AuthenticationHeaderHandler>()
                .AddScoped(sp => sp
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient(ClientName).EnableIntercept(sp))
                .AddHttpClient(ClientName, client =>
                {
                    client.DefaultRequestHeaders.AcceptLanguage.Clear();
                    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(CultureInfo.DefaultThreadCurrentCulture?.TwoLetterISOLanguageName);
                    client.BaseAddress = new Uri(gatewayUrl);
                })
                .AddHttpMessageHandler<AuthenticationHeaderHandler>();
            builder.Services.AddHttpClientInterceptor();
            return builder;
        }

        public static IServiceCollection AddManagers(this IServiceCollection services)
        {
            var managers = typeof(IManager);

            var types = managers
                .Assembly
                .GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Service = t.GetInterface($"I{t.Name}"),
                    Implementation = t
                })
                .Where(t => t.Service != null);

            foreach (var type in types)
            {
                if (managers.IsAssignableFrom(type.Service))
                {
                    services.AddScoped(type.Service, type.Implementation);
                }
            }

            return services;
        }

		public static IServiceCollection AddStores(this IServiceCollection services)
		{
			var managers = typeof(IStore);

			var types = managers
				.Assembly
				.GetExportedTypes()
				.Where(t => t.IsClass && !t.IsAbstract)
				.Select(t => new
				{
					Service = t.GetInterface($"I{t.Name}"),
					Implementation = t
				})
				.Where(t => t.Service != null);

			foreach (var type in types)
			{
				if (managers.IsAssignableFrom(type.Service))
				{
					services.AddScoped(type.Service, type.Implementation);
				}
			}

			return services;
		}

		private static void RegisterPermissionClaims(AuthorizationOptions options)
        {
            foreach (var prop in typeof(Permissions).GetNestedTypes().SelectMany(c => c.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
            {
                var propertyValue = prop.GetValue(null);
                if (propertyValue is not null)
                {
                    options.AddPolicy(propertyValue.ToString(), policy => policy.RequireClaim(ApplicationClaimTypes.Permission, propertyValue.ToString()));
                }
            }
        }
    }
}