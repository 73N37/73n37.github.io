using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;
using Microsoft.Extensions.Logging;

namespace AIDA.M365.Services;

/// <summary>
/// Mock calendar synchronization service for local/offline testing.
/// Simulates Graph API latency and failure scenarios.
/// </summary>
public sealed class DemoCalendarService : IGodsCalendarService
{
    private readonly ILogger<DemoCalendarService> _logger;

    public DemoCalendarService(ILogger<DemoCalendarService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<List<GodsEventCard>> FetchEventsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[DEMO Calendar] FetchEventsAsync called (no-op in demo mode).");
        return Task.FromResult(new List<GodsEventCard>());
    }

    /// <inheritdoc />
    public Task<string?> CreateEventAsync(GodsEventCard card, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[DEMO Calendar] CreateEventAsync '{Subject}' (no-op in demo mode).", card.Subject);
        return Task.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public Task UpdateEventAsync(GodsEventCard card, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[DEMO Calendar] UpdateEventAsync '{Subject}' (no-op in demo mode).", card.Subject);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task UpdateEventTimeAsync(
        string graphEventId,
        DateTimeOffset newStartUtc,
        DateTimeOffset newEndUtc,
        CancellationToken cancellationToken = default)
    {
        // 1.5 second delay to demonstrate the Ghost Card Preview spinner overlay
        await Task.Delay(1500, cancellationToken);

        if (graphEventId.Contains("fail", StringComparison.OrdinalIgnoreCase) || graphEventId == "event-4")
        {
            _logger.LogWarning("[DEMO Calendar] Simulated Graph API PATCH failure for {EventId}", graphEventId);
            throw new InvalidOperationException("M365 Graph service responded with 503 Service Unavailable.");
        }

        _logger.LogInformation(
            "[DEMO Calendar] Simulated PATCH {EventId} (Start={StartUtc}, End={EndUtc})",
            graphEventId, newStartUtc, newEndUtc);
    }

    /// <inheritdoc />
    public Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[DEMO Calendar] DeleteEventAsync {EventId} (no-op in demo mode).", graphEventId);
        return Task.CompletedTask;
    }
}
