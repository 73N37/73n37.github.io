namespace AIDA.M365.Models;

public sealed record MoveCardResult(
    bool Succeeded,
    string? ErrorMessage = null)
{
    public static MoveCardResult Success() => new(true);

    public static MoveCardResult Failure(string errorMessage) => new(false, errorMessage);
}
