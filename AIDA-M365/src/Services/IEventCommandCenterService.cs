using AIDA.M365.Models;

namespace AIDA.M365.Services;

public interface IEventCommandCenterService
{
    Task<MoveCardResult> MoveCardAsync(
        MoveCardRequest request,
        CancellationToken cancellationToken = default);

    Task<CardSummaryResult> SummarizeCardAsync(
        KanbanEventCard card,
        CancellationToken cancellationToken = default);
}
