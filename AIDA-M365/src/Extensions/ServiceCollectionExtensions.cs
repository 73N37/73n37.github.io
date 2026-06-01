using AIDA.M365.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIDA.M365.Extensions;

/// <summary>
/// Registers gods schedule management services for production cloud environments.
/// Uses Microsoft Outlook Calendar as the primary data source via Graph API.
/// Falls back to in-memory demo data when the user is not authenticated with Microsoft 365.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all AIDA services with live Outlook Calendar integration.
    /// </summary>
    public static IServiceCollection AddAidaKanbanServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Microsoft Graph calendar CRUD (read/write Outlook events)
        services.AddScoped<IGodsCalendarService, GraphCalendarService>();

        // Outlook as primary DB — falls back to demo data when not authenticated
        services.AddScoped<IGodsDatabaseService, OutlookGodsDatabaseService>();

        // Orchestration and ERP
        services.AddScoped<IGodsCommandCenterService, CommandCenterService>();
        services.AddScoped<IGodsErpService, EconomicErpService>();

        return services;
    }
}
