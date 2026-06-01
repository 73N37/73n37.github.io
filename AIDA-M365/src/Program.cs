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
    config.PopoverOptions.ThrowOnDuplicateProvider = false;
});

// ── Microsoft Entra ID / MSAL Authentication ──────────────────────────────
// Reads ClientId and Authority from appsettings.json → AzureAd section.
// ValidateAuthority: false is required for personal Microsoft accounts / new tenants.
//
// IMPORTANT for GitHub Pages:
// The redirect_uri must be registered in Azure as a SPA redirect URI.
// The login-callback path maps to a real static HTML file in wwwroot/authentication/
// which loads Blazor WASM and lets MSAL complete the token exchange.
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);

    // Request User.Read and Calendar access at login
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://graph.microsoft.com/User.Read");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://graph.microsoft.com/Calendars.ReadWrite");

    // Redirect mode (not popup) — works across all browsers including Safari ITP
    options.ProviderOptions.LoginMode = "redirect";
});

// ── Microsoft Graph SDK v5 ────────────────────────────────────────────────
// IMPORTANT: In Blazor WASM, GraphServiceClient must receive a pre-built
// HttpClient. The SDK's default constructor tries to configure
// SocketsHttpHandler.Proxy which throws PlatformNotSupportedException in
// the browser sandbox. Creating an HttpClient directly uses the browser's
// native fetch API instead (Blazor WASM's default transport).
builder.Services.AddScoped<IAuthenticationProvider, MsalGraphAuthenticationProvider>();
builder.Services.AddScoped(sp =>
{
    var authProvider = sp.GetRequiredService<IAuthenticationProvider>();
    // Use a plain HttpClient — in WASM this automatically uses BrowserHttpHandler
    var httpClient = new HttpClient { BaseAddress = new Uri("https://graph.microsoft.com/v1.0/") };
    return new GraphServiceClient(httpClient, authProvider, "https://graph.microsoft.com/v1.0");
});

// ── State Container ───────────────────────────────────────────────────────
builder.Services.AddScoped<IntegrationStateContainer>();

// ── Azure OpenAI ─────────────────────────────────────────────────────────
builder.Services.AddScoped<IAzureOpenAiService, AzureOpenAiService>();

// ── Production Outlook + Graph Services ──────────────────────────────────
// OutlookGodsDatabaseService reads live Outlook Calendar events when the
// user is logged in with Microsoft 365. Falls back to demo data when not.
builder.Services.AddAidaKanbanServices(builder.Configuration);

await builder.Build().RunAsync();
