using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIDA.M365.Services;

/// <summary>
/// Mock calendar synchronization service for local/offline testing of Engestofte Gods bookings.
/// Simulates Graph API network latency and failure scenarios.
/// </summary>
public sealed class DemoCalendarService : IGodsCalendarService
{
    private readonly ILogger<DemoCalendarService> _logger;

    public DemoCalendarService(ILogger<DemoCalendarService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task UpdateEventTimeAsync(
        string graphEventId,
        DateTimeOffset newStartUtc,
        DateTimeOffset newEndUtc,
        CancellationToken cancellationToken = default)
    {
        // 1.5 second delay to demonstrate the tactile Ghost Card Preview spinner overlay
        await Task.Delay(1500, cancellationToken);

        if (graphEventId.Contains("fail", StringComparison.OrdinalIgnoreCase) || graphEventId == "event-4")
        {
            _logger.LogWarning("[DEMO Calendar] Simulated Graph API PATCH failure for event {EventId}", graphEventId);
            throw new InvalidOperationException("M365 Graph service responded with 503 Service Unavailable.");
        }

        _logger.LogInformation(
            "[DEMO Calendar] Simulated Graph API PATCH for event {EventId}. Start={StartUtc}, End={EndUtc}",
            graphEventId,
            newStartUtc,
            newEndUtc);
    }
}
