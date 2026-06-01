using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace AIDA.M365.Services;

/// <summary>
/// Implements <see cref="IGodsDatabaseService"/> using Microsoft Outlook Calendar as the
/// live source of truth (via <see cref="IGodsCalendarService"/>).
///
/// When the user is authenticated with Microsoft 365:
///   → Reads/writes directly from/to their Outlook Calendar.
///   → AIDA metadata (kanban section, type, guests, price, etc.) is embedded inside
///     each Outlook event body as a hidden JSON marker — no extra database required.
///
/// When the user is NOT authenticated (no MS login):
///   → Transparently falls back to in-memory demo events so the app still renders.
///   → A banner prompts the user to log in.
/// </summary>
public sealed class OutlookGodsDatabaseService : IGodsDatabaseService
{
    private readonly IGodsCalendarService _calendar;
    private readonly AuthenticationStateProvider _authState;
    private readonly IntegrationStateContainer _stateContainer;
    private readonly ILogger<OutlookGodsDatabaseService> _logger;

    public OutlookGodsDatabaseService(
        IGodsCalendarService calendar,
        AuthenticationStateProvider authState,
        IntegrationStateContainer stateContainer,
        ILogger<OutlookGodsDatabaseService> logger)
    {
        _calendar       = calendar       ?? throw new ArgumentNullException(nameof(calendar));
        _authState      = authState      ?? throw new ArgumentNullException(nameof(authState));
        _stateContainer = stateContainer ?? throw new ArgumentNullException(nameof(stateContainer));
        _logger         = logger         ?? throw new ArgumentNullException(nameof(logger));
    }

    // ──────────────────────────────────────────────────────────────────────
    // FETCH
    // ──────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<List<GodsEventCard>> FetchAllEventsAsync(CancellationToken cancellationToken = default)
    {
        // If the user is logged into Microsoft 365, fetch live from Outlook
        if (await IsAuthenticatedAsync())
        {
            // Capture display name for the reconnect banner
            try
            {
                var authState = await _authState.GetAuthenticationStateAsync();
                var name = authState.User.Identity?.Name
                    ?? authState.User.FindFirst("preferred_username")?.Value
                    ?? authState.User.FindFirst("email")?.Value;
                _stateContainer.LoggedInUser = name;
            }
            catch { /* ignore – display name is non-critical */ }

            try
            {
                _logger.LogInformation("[Outlook DB] Fetching events from Outlook Calendar...");
                var events = await _calendar.FetchEventsAsync(cancellationToken);
                _stateContainer.Cards = events;
                _stateContainer.DidLastFetchSucceed = true;
                _stateContainer.IsOutlookConnected  = true;
                _stateContainer.NotifyStateChanged();
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Outlook DB] Live fetch failed, using demo fallback.");
                _stateContainer.DidLastFetchSucceed = false;
                _stateContainer.IsOutlookConnected  = false;
                // Do NOT clear Cards here — the user may have added events
                // optimistically via SaveEventAsync while this fetch was in-flight.
                // Clearing would destroy those cards (race condition).
                // Fall through to BuildDemoCards() only if Cards is truly empty.
            }
        }
        else
        {
            _logger.LogInformation("[Outlook DB] User not authenticated. Using demo fallback.");
            _stateContainer.IsOutlookConnected  = false;
            _stateContainer.DidLastFetchSucceed = true;
        }

        // Return cached cards if any exist; otherwise generate demo data in-memory
        if (_stateContainer.Cards.Count == 0)
        {
            _stateContainer.Cards = BuildDemoCards();
        }

        return _stateContainer.Cards;
    }

    // ──────────────────────────────────────────────────────────────────────
    // SAVE (create or update)
    // ──────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task SaveEventAsync(GodsEventCard card, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(card);

        // Optimistic UI update — add/replace in state immediately
        UpdateStateContainer(card);

        if (!await IsAuthenticatedAsync())
        {
            _logger.LogInformation("[Outlook DB] Offline: card '{Subject}' saved in-memory only.", card.Subject);
            // Still notify so components re-render with the optimistically-added card
            _stateContainer.NotifyStateChanged();
            return;
        }

        try
        {
            var isNewLocalCard = card.GraphEventId.StartsWith("evt-", StringComparison.Ordinal);

            if (isNewLocalCard)
            {
                // Create a brand-new Outlook event and get the real Graph ID
                var realId = await _calendar.CreateEventAsync(card, cancellationToken);
                if (!string.IsNullOrWhiteSpace(realId))
                {
                    // Replace the local placeholder card with one that has the real Outlook ID
                    var updatedCard = CloneWithRealId(card, realId);
                    RemoveFromStateContainer(card.GraphEventId);
                    UpdateStateContainer(updatedCard);
                    _logger.LogInformation("[Outlook DB] Created Outlook event {RealId} for '{Subject}'.", realId, card.Subject);
                }
            }
            else
            {
                // Update the existing Outlook event
                await _calendar.UpdateEventAsync(card, cancellationToken);
                _logger.LogInformation("[Outlook DB] Updated Outlook event {Id}.", card.GraphEventId);
            }

            // Mark connection as healthy since the save succeeded
            _stateContainer.IsOutlookConnected = true;
            _stateContainer.DidLastFetchSucceed = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Outlook DB] Failed to sync '{Subject}' to Outlook.", card.Subject);
            _stateContainer.IsOutlookConnected = false;
            _stateContainer.DidLastFetchSucceed = false;
        }

        _stateContainer.NotifyStateChanged();
    }

    // ──────────────────────────────────────────────────────────────────────
    // DELETE
    // ──────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphEventId))
            throw new ArgumentException("Event ID is required.", nameof(graphEventId));

        RemoveFromStateContainer(graphEventId);
        _stateContainer.NotifyStateChanged();

        // Only call Graph if this is a real Outlook ID (not a local placeholder)
        if (!graphEventId.StartsWith("evt-", StringComparison.Ordinal) && await IsAuthenticatedAsync())
        {
            try
            {
                await _calendar.DeleteEventAsync(graphEventId, cancellationToken);
                _logger.LogInformation("[Outlook DB] Deleted Outlook event {Id}.", graphEventId);
                _stateContainer.IsOutlookConnected = true;
                _stateContainer.DidLastFetchSucceed = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Outlook DB] Failed to delete Outlook event {Id}.", graphEventId);
                _stateContainer.IsOutlookConnected = false;
                _stateContainer.DidLastFetchSucceed = false;
                _stateContainer.NotifyStateChanged();
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // SYNC POSITION (kanban drag-drop)
    // ──────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task SyncCardPositionAsync(
        string graphEventId,
        string sectionKey,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        var card = _stateContainer.Cards.FirstOrDefault(c => c.GraphEventId == graphEventId);
        if (card is not null)
        {
            card.SectionKey = sectionKey;
            card.StartUtc   = start;
            card.EndUtc     = end;
            _stateContainer.NotifyStateChanged();
        }

        if (!await IsAuthenticatedAsync() || graphEventId.StartsWith("evt-", StringComparison.Ordinal))
            return;

        try
        {
            // Update both time and metadata (which carries the section key)
            if (card is not null)
                await _calendar.UpdateEventAsync(card, cancellationToken);
            else
                await _calendar.UpdateEventTimeAsync(graphEventId, start, end, cancellationToken);

            _stateContainer.IsOutlookConnected = true;
            _stateContainer.DidLastFetchSucceed = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Outlook DB] Failed to sync position for {Id}.", graphEventId);
            _stateContainer.IsOutlookConnected = false;
            _stateContainer.DidLastFetchSucceed = false;
            _stateContainer.NotifyStateChanged();
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // SEED (no-op: demo data is generated in-memory by FetchAllEventsAsync)
    // ──────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public Task SeedDemoDataToDbAsync(CancellationToken cancellationToken = default)
    {
        // When using Outlook as source of truth, seeding has no meaning.
        // FetchAllEventsAsync falls back to in-memory demo cards automatically
        // when the user is not authenticated.
        return Task.CompletedTask;
    }

    // ──────────────────────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ──────────────────────────────────────────────────────────────────────

    private async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var state = await _authState.GetAuthenticationStateAsync();
            return state.User.Identity?.IsAuthenticated == true;
        }
        catch
        {
            return false;
        }
    }

    private void UpdateStateContainer(GodsEventCard card)
    {
        var existing = _stateContainer.Cards.FirstOrDefault(c => c.GraphEventId == card.GraphEventId);
        if (existing is not null) _stateContainer.Cards.Remove(existing);
        _stateContainer.Cards.Add(card);
    }

    private void RemoveFromStateContainer(string graphEventId)
    {
        var existing = _stateContainer.Cards.FirstOrDefault(c => c.GraphEventId == graphEventId);
        if (existing is not null) _stateContainer.Cards.Remove(existing);
    }

    private static GodsEventCard CloneWithRealId(GodsEventCard src, string realId) => new()
    {
        GraphEventId        = realId,
        MetadataId          = src.MetadataId,
        Subject             = src.Subject,
        BodyContent         = src.BodyContent,
        SectionKey          = src.SectionKey,
        StartUtc            = src.StartUtc,
        EndUtc              = src.EndUtc,
        ColorHex            = src.ColorHex,
        Priority            = src.Priority,
        EventSubtype        = src.EventSubtype,
        GuestCount          = src.GuestCount,
        AssignedCoordinator = src.AssignedCoordinator,
        EstateArea          = src.EstateArea,
        CateringOption      = src.CateringOption,
        Price               = src.Price,
        SmartSummary        = src.SmartSummary,
        ActionItems         = src.ActionItems,
        SubEvents           = src.SubEvents
    };

    // ──────────────────────────────────────────────────────────────────────
    // DEMO DATA (shown when not logged in to Microsoft)
    // ──────────────────────────────────────────────────────────────────────

    private static List<GodsEventCard> BuildDemoCards()
    {
        var t = DateTimeOffset.UtcNow.Date.AddDays(1); // baseline: tomorrow UTC

        return
        [
            new()
            {
                GraphEventId        = "demo-event-001",
                MetadataId          = Guid.NewGuid(),
                Subject             = "Kensington Bryllup i Den Store Lade",
                BodyContent         = "Et eksklusivt gods-bryllup med fuld forplejning og velkomstchampagne i Søparken. (DEMO — Log ind med Microsoft for at se dine rigtige Outlook-begivenheder)",
                ColorHex            = "#D4AF37",
                Priority            = "High",
                SectionKey          = "confirmed",
                StartUtc            = t.AddDays(2).AddHours(11),
                EndUtc              = t.AddDays(2).AddHours(22),
                EventSubtype        = "Bryllup",
                GuestCount          = 120,
                AssignedCoordinator = "Sarah Jenkins",
                EstateArea          = "Den Store Lade",
                CateringOption      = "Gourmet Selskabsmenu",
                Price               = 185_000m,
                SubEvents           = [
                    new() { Title = "Velkomstreception & Champagne", StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(12, 30, 0), Location = "Søparken" },
                    new() { Title = "Bryllupsmiddag & Taler",        StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(17, 0, 0), Location = "Den Store Lade" }
                ]
            },
            new()
            {
                GraphEventId        = "demo-event-002",
                MetadataId          = Guid.NewGuid(),
                Subject             = "Sterling Konference i Hovedbygningen",
                BodyContent         = "Dagsmøde og konference for Manor Holdings. (DEMO)",
                ColorHex            = "#10B981",
                Priority            = "Normal",
                SectionKey          = "preparation",
                StartUtc            = t.AddDays(5).AddHours(8),
                EndUtc              = t.AddDays(5).AddHours(17),
                EventSubtype        = "Konference",
                GuestCount          = 80,
                AssignedCoordinator = "Michael Chang",
                EstateArea          = "Hovedbygningen",
                CateringOption      = "Konference-dagsmenu",
                Price               = 67_000m
            },
            new()
            {
                GraphEventId        = "demo-event-003",
                MetadataId          = Guid.NewGuid(),
                Subject             = "Midsommerfest i Søparken",
                BodyContent         = "Forespørgsel på have-reception og selskabsmiddag. (DEMO)",
                ColorHex            = "#8B5CF6",
                Priority            = "High",
                SectionKey          = "inquiry",
                StartUtc            = t.AddDays(10).AddHours(14),
                EndUtc              = t.AddDays(10).AddHours(23),
                EventSubtype        = "Privat Fest",
                GuestCount          = 200,
                AssignedCoordinator = "Sarah Jenkins",
                EstateArea          = "Søparken",
                CateringOption      = "Brunch & Champagne",
                Price               = 205_000m
            },
            new()
            {
                GraphEventId        = "demo-event-004",
                MetadataId          = Guid.NewGuid(),
                Subject             = "Jagt & Middag på Engestofte Gods",
                BodyContent         = "Eksklusiv jagt og middag-arrangement. (DEMO)",
                ColorHex            = "#EF4444",
                Priority            = "High",
                SectionKey          = "quoted",
                StartUtc            = t.AddDays(14).AddHours(9),
                EndUtc              = t.AddDays(14).AddHours(21),
                EventSubtype        = "Jagt & Event",
                GuestCount          = 40,
                AssignedCoordinator = "Lars Eriksen",
                EstateArea          = "Den Store Lade",
                CateringOption      = "Gourmet Selskabsmenu",
                Price               = 95_000m
            },
            new()
            {
                GraphEventId        = "demo-event-005",
                MetadataId          = Guid.NewGuid(),
                Subject             = "Royale Sølvbryllup — Familien Andersen",
                BodyContent         = "25-års jubilæumsbryllup med 160 gæster. (DEMO)",
                ColorHex            = "#D4AF37",
                Priority            = "High",
                SectionKey          = "confirmed",
                StartUtc            = t.AddDays(21).AddHours(15),
                EndUtc              = t.AddDays(21).AddHours(23),
                EventSubtype        = "Bryllup",
                GuestCount          = 160,
                AssignedCoordinator = "Anna Koordinator",
                EstateArea          = "Den Store Lade",
                CateringOption      = "Gourmet Selskabsmenu",
                Price               = 240_000m
            }
        ];
    }
}
