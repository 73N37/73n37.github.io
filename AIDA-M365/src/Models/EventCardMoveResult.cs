namespace AIDA.M365.Models;

/// <summary>
/// Captures the execution outcome of an event card move request.
/// </summary>
/// <param name="Succeeded">True if the move and all associated API integrations succeeded.</param>
/// <param name="ErrorMessage">Explanation of the failure, if any.</param>
public sealed record EventCardMoveResult(
    bool Succeeded,
    string? ErrorMessage = null)
{
    /// <summary>
    /// Generates a successful move result.
    /// </summary>
    public static EventCardMoveResult Success() => new(true);

    /// <summary>
    /// Generates a failed move result with a specific reason.
    /// </summary>
    public static EventCardMoveResult Failure(string errorMessage) => new(false, errorMessage);
}
