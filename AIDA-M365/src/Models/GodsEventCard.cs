using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIDA.M365.Models;

/// <summary>
/// Represents a cohesive scheduling and booking event at Engestofte Gods.
/// Holds Microsoft Graph calendar data, local database synchronization states, and e-conomic ERP invoice identifiers.
/// </summary>
public sealed class GodsEventCard
{
    /// <summary>
    /// Unique Graph calendar event identifier. Used as primary key.
    /// </summary>
    [JsonPropertyName("id")]
    public required string GraphEventId { get; init; }

    /// <summary>
    /// Guid metadata identifier for internal database tracking.
    /// </summary>
    [JsonPropertyName("metadata_id")]
    public Guid? MetadataId { get; init; }

    /// <summary>
    /// Heading/Title of the event (e.g. "Kensington Bryllup").
    /// </summary>
    [JsonPropertyName("subject")]
    public required string Subject { get; set; }

    /// <summary>
    /// Detailed description or body contents of the event.
    /// </summary>
    [JsonPropertyName("body_content")]
    public string? BodyContent { get; set; }

    /// <summary>
    /// Kanban columns key (e.g. "inquiry", "confirmed", "planning").
    /// </summary>
    [JsonPropertyName("section_key")]
    public required string SectionKey { get; set; }

    /// <summary>
    /// Start date and time of the booking in UTC.
    /// </summary>
    [JsonPropertyName("start_utc")]
    public required DateTimeOffset StartUtc { get; set; }

    /// <summary>
    /// End date and time of the booking in UTC.
    /// </summary>
    [JsonPropertyName("end_utc")]
    public required DateTimeOffset EndUtc { get; set; }

    /// <summary>
    /// Custom background theme color represented as a hex string.
    /// </summary>
    [JsonPropertyName("color_hex")]
    public string? ColorHex { get; set; }

    /// <summary>
    /// Priority level of the event (e.g. High, Medium, Low).
    /// </summary>
    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    /// <summary>
    /// AI-generated intelligent briefing summary of the event details.
    /// </summary>
    [JsonPropertyName("smart_summary")]
    public string? SmartSummary { get; set; }

    /// <summary>
    /// AI-generated actionable planning checklist items.
    /// </summary>
    [JsonPropertyName("action_items")]
    public IReadOnlyList<string> ActionItems { get; set; } = [];

    /// <summary>
    /// Ordered sequence of program items/sub-activities during the selskab.
    /// </summary>
    [JsonPropertyName("subevents")]
    public List<SubEvent> SubEvents { get; set; } = [];

    /// <summary>
    /// Selskab subtype matching Gods categories (e.g., Wedding, Conference, Celebration, HuntEvent).
    /// </summary>
    [JsonPropertyName("event_subtype")]
    public string? EventSubtype { get; set; }

    /// <summary>
    /// Registered count of attendees.
    /// </summary>
    [JsonPropertyName("guest_count")]
    public int GuestCount { get; set; }

    /// <summary>
    /// Internal coordinator assigned to manage the event logistics.
    /// </summary>
    [JsonPropertyName("assigned_coordinator")]
    public string? AssignedCoordinator { get; set; }

    /// <summary>
    /// Selected estate area (e.g. Den Store Lade, Hovedbygningen, Søparken).
    /// </summary>
    [JsonPropertyName("estate_area")]
    public string? EstateArea { get; set; }

    /// <summary>
    /// Selected dining options (e.g., Gourmet Selskabsmenu, Brunch & Champagne).
    /// </summary>
    [JsonPropertyName("catering_option")]
    public string? CateringOption { get; set; }

    /// <summary>
    /// Overall calculated pricing of the booking.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Booked e-conomic ERP invoice identifier.
    /// </summary>
    [JsonPropertyName("economic_invoice_number")]
    public int? EconomicInvoiceNumber { get; set; }

    /// <summary>
    /// Direct customer payment link generated from the booked e-conomic invoice.
    /// </summary>
    [JsonPropertyName("economic_payment_link")]
    public string? EconomicPaymentLink { get; set; }

    /// <summary>
    /// Returns true if an active e-conomic invoice has been booked.
    /// </summary>
    [JsonIgnore]
    public bool IsEconomicInvoiceBooked => EconomicInvoiceNumber.HasValue;

    /// <summary>
    /// Total timespan duration of the event.
    /// </summary>
    [JsonIgnore]
    public TimeSpan Duration => EndUtc - StartUtc;
}
