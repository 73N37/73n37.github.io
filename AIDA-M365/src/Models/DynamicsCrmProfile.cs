using System;

namespace AIDA.M365.Models;

public sealed class DynamicsCrmProfile
{
    public required string EntityId { get; init; }
    public required string EntityLogicalName { get; init; } // e.g. "lead", "opportunity", "account"
    public required string ClientName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Topic { get; set; } // e.g. "Wedding Booking 2026", "Golden Jubilee Birthday"
    public string? Status { get; set; } // e.g. "Qualified", "In Proposal", "Contract Signed"
    public decimal EstimatedBudget { get; set; }
    public string? PreferredVenue { get; set; } // e.g. "Grand Ballroom", "Rose Gardens"
    public string? Notes { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
}
