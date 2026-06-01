using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Authentication;
using MudBlazor.Services;
using AIDA.M365;
using AIDA.M365.Extensions;
using AIDA.M365.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── Default HttpClient ─────────────────────────────────────────────────────
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// ── MudBlazor ─────────────────────────────────────────────────────────────
builder.Services.AddMudServices(config =>
{
    // Suppress the "Duplicate MudPopoverProvider" exception during SPA navigation.
    config.PopoverOptions.ThrowOnDuplicateProvider = false;
});

// ── Microsoft Entra ID / MSAL Authentication ──────────────────────────────
// Reads ClientId and Authority from appsettings.json → AzureAd section.
// Requests Graph scopes silently on login so the app can access the calendar.
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://graph.microsoft.com/User.Read");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://graph.microsoft.com/Calendars.ReadWrite");
    options.ProviderOptions.LoginMode = "redirect";
});

// ── Microsoft Graph SDK v5 ────────────────────────────────────────────────
// MsalGraphAuthenticationProvider bridges the MSAL token to the Graph SDK.
builder.Services.AddScoped<IAuthenticationProvider, MsalGraphAuthenticationProvider>();
builder.Services.AddScoped(sp =>
{
    var authProvider = sp.GetRequiredService<IAuthenticationProvider>();
    return new GraphServiceClient(authProvider, "https://graph.microsoft.com/v1.0");
});

// ── State Container ───────────────────────────────────────────────────────
builder.Services.AddScoped<IntegrationStateContainer>();

// ── Azure OpenAI ─────────────────────────────────────────────────────────
builder.Services.AddScoped<IAzureOpenAiService, AzureOpenAiService>();

// ── Domain Services (Demo mode for local dev without full Graph token) ───
// Switch to AddAidaKanbanServices() to enable full production Graph sync.
builder.Services.AddAidaKanbanDemoServices();

await builder.Build().RunAsync();
