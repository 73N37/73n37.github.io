namespace AIDA.M365.Models;

public sealed record CardSummaryResult(
    bool Succeeded,
    string? Summary,
    IReadOnlyList<string> ActionItems,
    string? ErrorMessage = null)
{
    public static CardSummaryResult Success(string? summary, IReadOnlyList<string> actionItems) =>
        new(true, summary, actionItems);

    public static CardSummaryResult Failure(string errorMessage) =>
        new(false, null, [], errorMessage);
}
