using AIDA.M365.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AIDA.M365.Extensions;

/// <summary>
/// Provides extension methods to register gods schedule management services configured for offline sandbox/demo operations.
/// </summary>
public static class DemoServiceCollectionExtensions
{
    /// <summary>
    /// Registers Gods-centric services with offline demo mocks and active database synchronizers.
    /// </summary>
    public static IServiceCollection AddAidaKanbanDemoServices(this IServiceCollection services)
    {
        services.AddScoped<IGodsCalendarService, DemoCalendarService>();
        services.AddScoped<IGodsCommandCenterService, DemoCommandCenterService>();
        services.AddScoped<IGodsErpService, DemoErpService>();
        services.AddScoped<IGodsDatabaseService, GodsDatabaseService>();

        return services;
    }
}
