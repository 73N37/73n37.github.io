using System;
using System.Collections.Generic;

namespace AIDA.M365.Models;

public sealed class KanbanEventCard
{
    public required string GraphEventId { get; init; }

    public string? DynamicsEntityId { get; init; }

    public string? DynamicsEntityLogicalName { get; init; }

    public Guid? MetadataId { get; init; }

    public required string Subject { get; set; }

    public string? BodyContent { get; set; }

    public required string SectionKey { get; set; }

    // Store times in UTC to keep drag-drop updates deterministic.
    public required DateTimeOffset StartUtc { get; set; }

    public required DateTimeOffset EndUtc { get; set; }

    public string? ColorHex { get; set; }

    public string? Priority { get; set; }

    public string? SmartSummary { get; set; }

    public IReadOnlyList<string> ActionItems { get; set; } = [];

    public List<Subevent> Subevents { get; set; } = [];

    // Estate booking custom metadata properties for Proof of Concept
    public string? EventSubtype { get; set; } // e.g. "Wedding", "Birthday"
    
    public int GuestCount { get; set; }
    
    public string? AssignedCoordinator { get; set; }
    
    public string? EstateArea { get; set; } // e.g. "Grand Ballroom", "Rose Gardens", "Lakeside Pavilion"
    
    public string? CateringOption { get; set; } // e.g. "Fine Dining", "Buffet", "Champagne Brunch"
    
    public decimal? Price { get; set; } // Total price for the event in DKK

    // e-conomic ERP invoice integration state
    public int? EconomicInvoiceNumber { get; set; }
    public string? EconomicPaymentLink { get; set; }
    public bool IsEconomicInvoiceBooked => EconomicInvoiceNumber.HasValue;

    public TimeSpan Duration => EndUtc - StartUtc;

    public bool IsLinkedToDynamics =>
        !string.IsNullOrWhiteSpace(DynamicsEntityId)
        && !string.IsNullOrWhiteSpace(DynamicsEntityLogicalName);
}
