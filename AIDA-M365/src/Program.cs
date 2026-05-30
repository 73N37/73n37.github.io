using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using AIDA.M365;
using AIDA.M365.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<AIDA.M365.Services.IntegrationStateContainer>();

// Use the demo services for local viewing without requiring a real Graph connection
builder.Services.AddAidaKanbanDemoServices();

await builder.Build().RunAsync();
