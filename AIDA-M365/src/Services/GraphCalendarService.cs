using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace AIDA.M365.Services;

/// <summary>
/// Production Microsoft Graph synchronization service.
/// Reads and writes real Outlook Calendar events, embedding AIDA-specific
/// metadata (kanban section, event type, guest count, etc.) as a JSON marker
/// in the event body so it persists inside Outlook itself — no separate database needed.
///
/// Metadata is stored as an HTML comment at the end of the event body:
///   <!--AIDA:{"sectionKey":"confirmed","eventSubtype":"Bryllup",...}-->
/// </summary>
public sealed class GraphCalendarService : IGodsCalendarService
{
    private const string AidaMarkerStart = "<!--AIDA:";
    private const string AidaMarkerEnd   = "-->";
    private const string ExtensionName   = "com.engestofte.aida";

    private readonly GraphServiceClient _graph;
    private readonly ILogger<GraphCalendarService> _logger;

    public GraphCalendarService(GraphServiceClient graph, ILogger<GraphCalendarService> logger)
    {
        _graph  = graph  ?? throw new ArgumentNullException(nameof(graph));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ──────────────────────────────────────────────────────────────────────
    // FETCH
    // ──────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<List<GodsEventCard>> FetchEventsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Graph] Fetching Outlook calendar events...");

        var cards  = new List<GodsEventCard>();
        var result = new List<GodsEventCard>();

        try
        {
            // Fetch next 12 months of events from Outlook
            var now    = DateTime.UtcNow;
            var future = now.AddMonths(12);

            var page = await _graph.Me.CalendarView.GetAsync(config =>
            {
                config.QueryParameters.StartDateTime = now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                config.QueryParameters.EndDateTime   = future.ToString("yyyy-MM-ddTHH:mm:ssZ");
                config.QueryParameters.Select        = ["id", "subject", "body", "start", "end"];
                config.QueryParameters.Top           = 100;
                config.QueryParameters.Orderby       = ["start/dateTime"];
            }, cancellationToken);

            var events = page?.Value ?? [];
            foreach (var ev in events)
            {
                if (ev is null) continue;
                var card = MapToCard(ev);
                if (card is not null) result.Add(card);
            }

            _logger.LogInformation("[Graph] Fetched {Count} Outlook events.", result.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Graph] Failed to fetch calendar events.");
        }

        return result;
    }

    // ──────────────────────────────────────────────────────────────────────
    // CREATE
    // ──────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<string?> CreateEventAsync(GodsEventCard card, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Graph] Creating Outlook event: {Subject}", card.Subject);

        try
        {
            var graphEvent = new Event
            {
                Subject = card.Subject,
                Body    = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content     = BuildBodyHtml(card)
                },
                Start = ToDateTimeTimeZone(card.StartUtc),
                End   = ToDateTimeTimeZone(card.EndUtc)
            };

            var created = await _graph.Me.Events.PostAsync(graphEvent, cancellationToken: cancellationToken);
            var realId  = created?.Id;
            _logger.LogInformation("[Graph] Created Outlook event {Id} for '{Subject}'.", realId, card.Subject);
            return realId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Graph] Failed to create event '{Subject}'.", card.Subject);
            return null;
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // UPDATE (full)
    // ──────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task UpdateEventAsync(GodsEventCard card, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(card.GraphEventId) || card.GraphEventId.StartsWith("evt-"))
            return;

        _logger.LogInformation("[Graph] Updating Outlook event {Id}: {Subject}", card.GraphEventId, card.Subject);

        try
        {
            var patch = new Event
            {
                Subject = card.Subject,
                Body    = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content     = BuildBodyHtml(card)
                },
                Start = ToDateTimeTimeZone(card.StartUtc),
                End   = ToDateTimeTimeZone(card.EndUtc)
            };

            await _graph.Me.Events[card.GraphEventId]
                .PatchAsync(patch, cancellationToken: cancellationToken);

            _logger.LogInformation("[Graph] Updated Outlook event {Id}.", card.GraphEventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Graph] Failed to update event {Id}.", card.GraphEventId);
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // UPDATE TIME (lightweight)
    // ──────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task UpdateEventTimeAsync(
        string graphEventId,
        DateTimeOffset newStartUtc,
        DateTimeOffset newEndUtc,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphEventId) || graphEventId.StartsWith("evt-"))
            throw new ArgumentException("Valid Graph event id required.", nameof(graphEventId));

        var patch = new Event
        {
            Start = ToDateTimeTimeZone(newStartUtc),
            End   = ToDateTimeTimeZone(newEndUtc)
        };

        _logger.LogInformation("[Graph] Patching time on Outlook event {Id}.", graphEventId);

        await _graph.Me.Events[graphEventId]
            .PatchAsync(patch, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    // ──────────────────────────────────────────────────────────────────────
    // DELETE
    // ──────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphEventId) || graphEventId.StartsWith("evt-"))
            return;

        _logger.LogInformation("[Graph] Deleting Outlook event {Id}.", graphEventId);

        try
        {
            await _graph.Me.Events[graphEventId]
                .DeleteAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Graph] Failed to delete event {Id}.", graphEventId);
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Maps a Microsoft Graph Event to a GodsEventCard, extracting AIDA metadata
    /// embedded in the event body as a JSON comment.
    /// </summary>
    private static GodsEventCard? MapToCard(Event ev)
    {
        if (ev?.Id is null) return null;

        var bodyHtml = ev.Body?.Content ?? "";

        // Extract AIDA metadata JSON block from the event body
        AidaMeta meta = ExtractMeta(bodyHtml);

        // Strip the AIDA marker from the visible body preview
        var cleanBody = StripAidaMarker(bodyHtml);

        return new GodsEventCard
        {
            GraphEventId        = ev.Id,
            MetadataId          = Guid.NewGuid(),
            Subject             = ev.Subject ?? "(Ingen titel)",
            BodyContent         = cleanBody,
            SectionKey          = meta.SectionKey ?? "confirmed",
            StartUtc            = ParseGraphDateTime(ev.Start),
            EndUtc              = ParseGraphDateTime(ev.End),
            ColorHex            = meta.ColorHex ?? "#10B981",
            Priority            = meta.Priority ?? "Normal",
            EventSubtype        = meta.EventSubtype,
            GuestCount          = meta.GuestCount,
            AssignedCoordinator = meta.AssignedCoordinator,
            EstateArea          = meta.EstateArea,
            CateringOption      = meta.CateringOption,
            Price               = meta.Price > 0 ? meta.Price : null
        };
    }

    private static readonly System.Text.Json.JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };

    private static string BuildBodyHtml(GodsEventCard card)
    {
        var visible = card.BodyContent ?? $"Selskab oprettet fra AIDA Estate Command. Type: {card.EventSubtype}.";

        var meta = new AidaMeta
        {
            SectionKey          = card.SectionKey,
            EventSubtype        = card.EventSubtype,
            GuestCount          = card.GuestCount,
            AssignedCoordinator = card.AssignedCoordinator,
            EstateArea          = card.EstateArea,
            CateringOption      = card.CateringOption,
            Price               = card.Price ?? 0,
            ColorHex            = card.ColorHex,
            Priority            = card.Priority
        };

        var json = System.Text.Json.JsonSerializer.Serialize(meta, _jsonOpts);
        return $"<p>{visible}</p>{AidaMarkerStart}{json}{AidaMarkerEnd}";
    }

    private static AidaMeta ExtractMeta(string bodyHtml)
    {
        var start = bodyHtml.IndexOf(AidaMarkerStart, StringComparison.Ordinal);
        if (start < 0) return new AidaMeta();

        var jsonStart = start + AidaMarkerStart.Length;
        var end       = bodyHtml.IndexOf(AidaMarkerEnd, jsonStart, StringComparison.Ordinal);
        if (end < 0) return new AidaMeta();

        var json = bodyHtml[jsonStart..end];
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<AidaMeta>(json, _jsonOpts) ?? new AidaMeta();
        }
        catch
        {
            return new AidaMeta();
        }
    }

    private static string StripAidaMarker(string html)
    {
        var start = html.IndexOf(AidaMarkerStart, StringComparison.Ordinal);
        return start < 0 ? html : html[..start].Trim();
    }

    private static DateTimeOffset ParseGraphDateTime(DateTimeTimeZone? dt)
    {
        if (dt?.DateTime is null) return DateTimeOffset.UtcNow;
        return DateTimeOffset.TryParse(dt.DateTime, null, System.Globalization.DateTimeStyles.AssumeUniversal, out var result)
            ? result
            : DateTimeOffset.UtcNow;
    }

    private static DateTimeTimeZone ToDateTimeTimeZone(DateTimeOffset dt) => new()
    {
        DateTime = dt.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
        TimeZone = "UTC"
    };

    internal sealed class AidaMeta
    {
        public string?  SectionKey          { get; set; }
        public string?  EventSubtype        { get; set; }
        public int      GuestCount          { get; set; }
        public string?  AssignedCoordinator { get; set; }
        public string?  EstateArea          { get; set; }
        public string?  CateringOption      { get; set; }
        public decimal  Price               { get; set; }
        public string?  ColorHex            { get; set; }
        public string?  Priority            { get; set; }
    }
}
