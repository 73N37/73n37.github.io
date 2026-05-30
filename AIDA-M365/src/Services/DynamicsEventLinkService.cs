using System.Net.Http.Json;
using AIDA.M365.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIDA.M365.Services;

public sealed class DynamicsEventLinkService : IDynamicsEventLinkService
{
    private readonly HttpClient _httpClient;
    private readonly DynamicsOptions _options;
    private readonly ILogger<DynamicsEventLinkService> _logger;

    public DynamicsEventLinkService(
        HttpClient httpClient,
        IOptions<DynamicsOptions> options,
        ILogger<DynamicsEventLinkService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task UpdateLinkedEventAsync(
        DynamicsEventUpdate update,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.EnvironmentUrl))
        {
            throw new InvalidOperationException("Dynamics environment URL is not configured.");
        }

        var entitySetName = ToEntitySetName(update.EntityLogicalName);
        var uri = $"{_options.EnvironmentUrl.TrimEnd('/')}/api/data/{_options.ApiVersion}/{entitySetName}({update.EntityId})";

        using var request = new HttpRequestMessage(HttpMethod.Patch, uri)
        {
            Content = JsonContent.Create(new
            {
                aida_grapheventid = update.GraphEventId,
                aida_kanbansection = update.SectionKey,
                aida_startutc = update.StartUtc.UtcDateTime,
                aida_endutc = update.EndUtc.UtcDateTime
            })
        };

        _logger.LogInformation(
            "Updating Dynamics {EntityLogicalName} {EntityId} for Graph event {EventId}",
            update.EntityLogicalName,
            update.EntityId,
            update.GraphEventId);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static string ToEntitySetName(string logicalName) =>
        logicalName.ToLowerInvariant() switch
        {
            "account" => "accounts",
            "opportunity" => "opportunities",
            "lead" => "leads",
            var value when value.EndsWith('y') => $"{value[..^1]}ies",
            var value when value.EndsWith('s') => value,
            var value => $"{value}s"
        };
}
