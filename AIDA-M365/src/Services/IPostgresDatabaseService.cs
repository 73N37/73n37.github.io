using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;

namespace AIDA.M365.Services;

public interface IPostgresDatabaseService
{
    Task<List<KanbanEventCard>> FetchAllEventsAsync(CancellationToken cancellationToken = default);
    Task SaveEventAsync(KanbanEventCard card, CancellationToken cancellationToken = default);
    Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default);
    Task SyncCardPositionAsync(string graphEventId, string sectionKey, System.DateTimeOffset start, System.DateTimeOffset end, CancellationToken cancellationToken = default);
    Task SeedDemoDataToDbAsync(CancellationToken cancellationToken = default);
}
