using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIDA.M365.Services;

public sealed class DemoOutlookCalendarEventService : IOutlookCalendarEventService
{
    private readonly ILogger<DemoOutlookCalendarEventService> _logger;

    public DemoOutlookCalendarEventService(ILogger<DemoOutlookCalendarEventService> logger)
    {
        _logger = logger;
    }

    public async Task UpdateEventTimeAsync(
        string graphEventId,
        DateTimeOffset newStartUtc,
        DateTimeOffset newEndUtc,
        CancellationToken cancellationToken = default)
    {
        // 1.5 second delay to demonstrate the Ghost Card Preview spinner
        await Task.Delay(1500, cancellationToken);

        if (graphEventId.Contains("fail", StringComparison.OrdinalIgnoreCase) || graphEventId == "event-4")
        {
            _logger.LogWarning("[DEMO] Simulated Graph PATCH failure for event {EventId}", graphEventId);
            throw new InvalidOperationException("M365 Graph service responded with 503 Service Unavailable.");
        }

        _logger.LogInformation(
            "[DEMO] Simulated Graph PATCH for event {EventId}. Start={StartUtc}, End={EndUtc}",
            graphEventId,
            newStartUtc,
            newEndUtc);
    }
}
