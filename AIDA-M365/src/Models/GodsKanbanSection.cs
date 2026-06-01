namespace AIDA.M365.Models;

/// <summary>
/// Defines a workflow stage column (e.g. Henvendelser, Bekræftede Bookinger, Planlægning) on the gods planning dashboard.
/// </summary>
public sealed class GodsKanbanSection
{
    /// <summary>
    /// Unique workflow state key matching the section identifier.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// User-friendly header label of the column.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Day offset from UTC today used for default event scheduling during column-drag.
    /// </summary>
    public int DayOffsetFromTodayUtc { get; init; }

    /// <summary>
    /// Default scheduling start hour in UTC.
    /// </summary>
    public int StartHourUtc { get; init; }

    /// <summary>
    /// Default scheduling start minute in UTC.
    /// </summary>
    public int StartMinuteUtc { get; init; }
}
