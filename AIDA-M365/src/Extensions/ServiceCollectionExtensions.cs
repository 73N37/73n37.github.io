using AIDA.M365.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIDA.M365.Extensions;

/// <summary>
/// Provides extension methods to register gods schedule management services configured for production cloud environments.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Gods-centric services with production Graph API, Dynamics 365, and e-conomic integrations.
    /// </summary>
    public static IServiceCollection AddAidaKanbanServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IGodsCalendarService, GraphCalendarService>();
        services.AddScoped<IGodsCommandCenterService, CommandCenterService>();
        services.AddScoped<IGodsErpService, EconomicErpService>();
        services.AddScoped<IGodsDatabaseService, GodsDatabaseService>();

        return services;
    }
}
