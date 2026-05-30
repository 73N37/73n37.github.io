namespace AIDA.M365.Models;

public sealed record MoveCardRequest(
    KanbanEventCard Card,
    string TargetSectionKey,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc);
