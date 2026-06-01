using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;

namespace AIDA.M365.Services;

/// <summary>
/// Provides full CRUD calendar synchronization with Microsoft Outlook Calendar via Graph API.
/// </summary>
public interface IGodsCalendarService
{
    /// <summary>
    /// Fetches all calendar events from the user's Outlook Calendar and maps them to GodsEventCards.
    /// </summary>
    Task<List<GodsEventCard>> FetchEventsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new Outlook calendar event from a GodsEventCard.
    /// Returns the real Graph event ID assigned by Outlook.
    /// </summary>
    Task<string?> CreateEventAsync(GodsEventCard card, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing Outlook calendar event's time, title, and AIDA metadata.
    /// </summary>
    Task UpdateEventAsync(GodsEventCard card, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates only the scheduled date and time range of a calendar event (lightweight patch).
    /// </summary>
    Task UpdateEventTimeAsync(
        string graphEventId,
        DateTimeOffset newStartUtc,
        DateTimeOffset newEndUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes an Outlook calendar event.
    /// </summary>
    Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default);
}
