using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace AIDA.M365.Services;

public sealed class OutlookCalendarEventService : IOutlookCalendarEventService
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly ILogger<OutlookCalendarEventService> _logger;

    public OutlookCalendarEventService(
        GraphServiceClient graphServiceClient,
        ILogger<OutlookCalendarEventService> logger)
    {
        _graphServiceClient = graphServiceClient;
        _logger = logger;
    }

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
            throw new ArgumentException("End must be greater than start.", nameof(newEndUtc));
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
            "Updating Outlook event {EventId} with Start={StartUtc} and End={EndUtc}",
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
