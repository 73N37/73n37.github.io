using System;

namespace AIDA.M365.Models;

public sealed class Subevent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public required string Title { get; set; }
    
    public required TimeSpan StartTime { get; set; }
    
    public required TimeSpan EndTime { get; set; }
    
    public required string Location { get; set; }
}
