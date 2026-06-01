using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace AIDA.M365.Services;

/// <summary>
/// Production Microsoft Graph synchronization service for gods calendar events.
/// Updates real Outlook Shared Calendar events using Graph API v5.
/// </summary>
public sealed class GraphCalendarService : IGodsCalendarService
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly ILogger<GraphCalendarService> _logger;

    public GraphCalendarService(
        GraphServiceClient graphServiceClient,
        ILogger<GraphCalendarService> logger)
    {
        _graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task UpdateEventTimeAsync(
        string graphEventId,
        DateTimeOffset newStartUtc,
        DateTimeOffset newEndUtc,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphEventId))
        {
            throw new ArgumentException("Graph event id is required.", nameof(graphEventId));
        }

        if (newEndUtc <= newStartUtc)
        {
            throw new ArgumentException("End date must be greater than start date.", nameof(newEndUtc));
        }

        var patch = new Event
        {
            Start = new DateTimeTimeZone
            {
                DateTime = newStartUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                TimeZone = "UTC"
            },
            End = new DateTimeTimeZone
            {
                DateTime = newEndUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                TimeZone = "UTC"
            }
        };

        _logger.LogInformation(
            "[Graph Calendar] Patching real Outlook event {EventId} (Start={StartUtc}, End={EndUtc})",
            graphEventId,
            newStartUtc,
            newEndUtc);

        await _graphServiceClient
            .Me
            .Events[graphEventId]
            .PatchAsync(patch, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
