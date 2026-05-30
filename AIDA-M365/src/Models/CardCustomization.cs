namespace AIDA.M365.Models;

public sealed record CardCustomization(
    Guid? MetadataId,
    string GraphEventId,
    string SectionKey,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    string? ColorHex,
    string? Priority,
    string? SmartSummary,
    IReadOnlyList<string> ActionItems,
    DateTimeOffset UpdatedUtc);
