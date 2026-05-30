using AIDA.M365.Models;

namespace AIDA.M365.Services;

public interface IAiCardSummaryService
{
    Task<CardSummaryResult> SummarizeAsync(
        KanbanEventCard card,
        CancellationToken cancellationToken = default);
}
