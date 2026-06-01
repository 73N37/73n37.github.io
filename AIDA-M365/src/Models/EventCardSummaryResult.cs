using System.Collections.Generic;

namespace AIDA.M365.Models;

/// <summary>
/// Captures the outcome of a text summarization and action item extraction task.
/// </summary>
/// <param name="Succeeded">True if the summarization task succeeded.</param>
/// <param name="Summary">The generated summary text.</param>
/// <param name="ActionItems">List of extracted checkbox checklist items.</param>
/// <param name="ErrorMessage">Explanation of the failure, if any.</param>
public sealed record EventCardSummaryResult(
    bool Succeeded,
    string? Summary,
    IReadOnlyList<string> ActionItems,
    string? ErrorMessage = null)
{
    /// <summary>
    /// Generates a successful summary outcome.
    /// </summary>
    public static EventCardSummaryResult Success(string? summary, IReadOnlyList<string> actionItems) =>
        new(true, summary, actionItems);

    /// <summary>
    /// Generates a failed summary outcome.
    /// </summary>
    public static EventCardSummaryResult Failure(string errorMessage) =>
        new(false, null, [], errorMessage);
}
