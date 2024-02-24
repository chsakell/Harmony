using Harmony.Application.Extensions;
using Harmony.Client;
using Harmony.Client.Extensions;
using Harmony.Client.Infrastructure.Authentication;
using Harmony.Client.Infrastructure.Managers.Preferences;
using Harmony.Client.Infrastructure.Managers.Project;
using Harmony.Client.Infrastructure.Settings;
using Harmony.Shared.Constants.Localization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Polly.Registry;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.AddClientServices(builder.Configuration);
builder.Services.AddRetryPolicies();

var host = builder.Build();

var storageService = host.Services.GetRequiredService<ClientPreferenceManager>();
if (storageService != null)
{
    CultureInfo culture;
    var preference = await storageService.GetPreference() as ClientPreference;
    if (preference != null)
        culture = new CultureInfo(preference.LanguageCode);
    else
        culture = new CultureInfo(LocalizationConstants.SupportedLanguages.FirstOrDefault()?.Code ?? "en-US");
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}

var stateProvider = host.Services.GetService<HarmonyStateProvider>();
var workspaceManager = host.Services.GetService<IWorkspaceManager>();

var state = await stateProvider.GetAuthenticationStateAsync();
var isAuthenticated = state?.User?.Identity?.IsAuthenticated == true;

if(isAuthenticated)
{
    try
    {
        await workspaceManager.InitAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
    
}
await host.RunAsync();
