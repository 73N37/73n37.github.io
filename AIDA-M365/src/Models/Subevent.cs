using System;

namespace AIDA.M365.Models;

/// <summary>
/// Represents a sub-event or program point (e.g. reception, dinner, dance) within a larger estate booking.
/// </summary>
public sealed class SubEvent
{
    /// <summary>
    /// Unique identifier for the sub-event.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Title or name of the sub-event (e.g. "Bryllupsreception").
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Start time offset of the sub-event relative to the main event date.
    /// </summary>
    public required TimeSpan StartTime { get; set; }
    
    /// <summary>
    /// End time offset of the sub-event relative to the main event date.
    /// </summary>
    public required TimeSpan EndTime { get; set; }
    
    /// <summary>
    /// Location of the sub-event within the estate (e.g. "Den Store Lade", "Søparken").
    /// </summary>
    public required string Location { get; set; }
}
