using AIDA.M365.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIDA.M365.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAidaKanbanServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AzureOpenAiOptions>(options =>
        {
            var section = configuration.GetSection("AzureOpenAI");
            options.Endpoint = section[nameof(AzureOpenAiOptions.Endpoint)] ?? string.Empty;
            options.DeploymentName = section[nameof(AzureOpenAiOptions.DeploymentName)] ?? string.Empty;
            options.ApiVersion = section[nameof(AzureOpenAiOptions.ApiVersion)] ?? options.ApiVersion;
            options.ApiKey = section[nameof(AzureOpenAiOptions.ApiKey)];
        });

        services.AddScoped<IOutlookCalendarEventService, OutlookCalendarEventService>();
        services.AddScoped<ICardCustomizationRepository, CardCustomizationApiRepository>();
        services.AddScoped<IAiCardSummaryService, AzureOpenAiCardSummaryService>();
        services.AddScoped<IEventCommandCenterService, EventCommandCenterService>();
        services.AddScoped<IEconomicErpService, EconomicErpService>();
        services.AddScoped<IBookingAcknowledgementService, BookingAcknowledgementService>();

        return services;
    }
}
