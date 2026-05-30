namespace AIDA.M365.Models;

public sealed record DynamicsEventUpdate(
    string EntityLogicalName,
    string EntityId,
    string GraphEventId,
    string SectionKey,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc);
