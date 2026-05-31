using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIDA.M365.Models;

public sealed class KanbanEventCard
{
    [JsonPropertyName("id")]
    public required string GraphEventId { get; init; }

    [JsonPropertyName("dynamicsEntityId")]
    public string? DynamicsEntityId { get; init; }

    [JsonPropertyName("dynamicsEntityLogicalName")]
    public string? DynamicsEntityLogicalName { get; init; }

    [JsonPropertyName("metadataId")]
    public Guid? MetadataId { get; init; }

    [JsonPropertyName("subject")]
    public required string Subject { get; set; }

    [JsonPropertyName("bodyContent")]
    public string? BodyContent { get; set; }

    [JsonPropertyName("sectionKey")]
    public required string SectionKey { get; set; }

    // Store times in UTC to keep drag-drop updates deterministic.
    [JsonPropertyName("startUtc")]
    public required DateTimeOffset StartUtc { get; set; }

    [JsonPropertyName("endUtc")]
    public required DateTimeOffset EndUtc { get; set; }

    [JsonPropertyName("colorHex")]
    public string? ColorHex { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("smartSummary")]
    public string? SmartSummary { get; set; }

    [JsonPropertyName("actionItems")]
    public IReadOnlyList<string> ActionItems { get; set; } = [];

    [JsonPropertyName("subevents")]
    public List<Subevent> Subevents { get; set; } = [];

    // Estate booking custom metadata properties
    [JsonPropertyName("eventSubtype")]
    public string? EventSubtype { get; set; }

    [JsonPropertyName("guestCount")]
    public int GuestCount { get; set; }

    [JsonPropertyName("assignedCoordinator")]
    public string? AssignedCoordinator { get; set; }

    [JsonPropertyName("estateArea")]
    public string? EstateArea { get; set; }

    [JsonPropertyName("cateringOption")]
    public string? CateringOption { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    // e-conomic ERP invoice integration state
    [JsonPropertyName("economicInvoiceNumber")]
    public int? EconomicInvoiceNumber { get; set; }

    [JsonPropertyName("economicPaymentLink")]
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
