using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIDA.M365.Models;

public sealed class KanbanEventCard
{
    [JsonPropertyName("id")]
    public required string GraphEventId { get; init; }

    [JsonPropertyName("dynamics_entity_id")]
    public string? DynamicsEntityId { get; init; }

    [JsonPropertyName("dynamics_entity_logical_name")]
    public string? DynamicsEntityLogicalName { get; init; }

    [JsonPropertyName("metadata_id")]
    public Guid? MetadataId { get; init; }

    [JsonPropertyName("subject")]
    public required string Subject { get; set; }

    [JsonPropertyName("body_content")]
    public string? BodyContent { get; set; }

    [JsonPropertyName("section_key")]
    public required string SectionKey { get; set; }

    // Store times in UTC to keep drag-drop updates deterministic.
    [JsonPropertyName("start_utc")]
    public required DateTimeOffset StartUtc { get; set; }

    [JsonPropertyName("end_utc")]
    public required DateTimeOffset EndUtc { get; set; }

    [JsonPropertyName("color_hex")]
    public string? ColorHex { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("smart_summary")]
    public string? SmartSummary { get; set; }

    [JsonPropertyName("action_items")]
    public IReadOnlyList<string> ActionItems { get; set; } = [];

    [JsonPropertyName("subevents")]
    public List<Subevent> Subevents { get; set; } = [];

    // Estate booking custom metadata properties
    [JsonPropertyName("event_subtype")]
    public string? EventSubtype { get; set; }

    [JsonPropertyName("guest_count")]
    public int GuestCount { get; set; }

    [JsonPropertyName("assigned_coordinator")]
    public string? AssignedCoordinator { get; set; }

    [JsonPropertyName("estate_area")]
    public string? EstateArea { get; set; }

    [JsonPropertyName("catering_option")]
    public string? CateringOption { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    // e-conomic ERP invoice integration state
    [JsonPropertyName("economic_invoice_number")]
    public int? EconomicInvoiceNumber { get; set; }

    [JsonPropertyName("economic_payment_link")]
    public string? EconomicPaymentLink { get; set; }

    [JsonIgnore]
    public bool IsEconomicInvoiceBooked => EconomicInvoiceNumber.HasValue;

    [JsonIgnore]
    public TimeSpan Duration => EndUtc - StartUtc;

    [JsonIgnore]
    public bool IsLinkedToDynamics =>
        !string.IsNullOrWhiteSpace(DynamicsEntityId)
        && !string.IsNullOrWhiteSpace(DynamicsEntityLogicalName);
}
