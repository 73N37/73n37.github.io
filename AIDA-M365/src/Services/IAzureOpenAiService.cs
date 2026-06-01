using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;

namespace AIDA.M365.Services;

/// <summary>
/// Contract for Azure OpenAI-powered event card summarization.
/// </summary>
public interface IAzureOpenAiService
{
    Task<EventCardSummaryResult> SummarizeCardAsync(
        GodsEventCard card,
        CancellationToken cancellationToken = default);
}
