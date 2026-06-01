using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;

namespace AIDA.M365.Services;

/// <summary>
/// Orchestrates gods scheduling workflow automations, stage transitions, and ERP invoicing actions.
/// </summary>
public interface IGodsCommandCenterService
{
    /// <summary>
    /// Executes card drag-and-drop operations, triggering e-conomic invoice provisioning when card moves to confirmed.
    /// </summary>
    Task<EventCardMoveResult> MoveCardAsync(
        EventCardMoveRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates intelligent summaries and checklists using the active AI assistant.
    /// </summary>
    Task<EventCardSummaryResult> SummarizeCardAsync(
        GodsEventCard card,
        CancellationToken cancellationToken = default);
}
