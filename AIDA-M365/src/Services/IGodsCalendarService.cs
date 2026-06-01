using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIDA.M365.Services;

/// <summary>
/// Provides calendar synchronization operations with Microsoft Outlook Calendar via Graph API.
/// </summary>
public interface IGodsCalendarService
{
    /// <summary>
    /// Updates the scheduled date and time range of a calendar event.
    /// </summary>
    Task UpdateEventTimeAsync(
        string graphEventId,
        DateTimeOffset newStartUtc,
        DateTimeOffset newEndUtc,
        CancellationToken cancellationToken = default);
}
