using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIDA.M365.Services;

public interface IOutlookCalendarEventService
{
    Task UpdateEventTimeAsync(
        string graphEventId,
        DateTimeOffset newStartUtc,
        DateTimeOffset newEndUtc,
        CancellationToken cancellationToken = default);
}
