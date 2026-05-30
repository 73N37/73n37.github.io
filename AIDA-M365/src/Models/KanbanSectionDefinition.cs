namespace AIDA.M365.Models;

public sealed class KanbanSectionDefinition
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    // Day offset from UTC today (e.g. 0 = today, 1 = tomorrow).
    public int DayOffsetFromTodayUtc { get; init; }

    public int StartHourUtc { get; init; }

    public int StartMinuteUtc { get; init; }
}
