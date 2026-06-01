using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;
using Microsoft.Extensions.Logging;

namespace AIDA.M365.Services;

/// <summary>
/// Core database synchronization service for Engestofte Gods bookings.
/// Orchestrates reads and writes to local database containers (via PostgREST), Supabase PostgreSQL,
/// and Azure SQL database endpoints (via Data API Builder), with automatic sandbox in-memory cache fallback.
/// </summary>
public sealed class GodsDatabaseService : IGodsDatabaseService
{
    private readonly HttpClient _httpClient;
    private readonly IntegrationStateContainer _stateContainer;
    private readonly ILogger<GodsDatabaseService> _logger;

    public GodsDatabaseService(
        HttpClient httpClient,
        IntegrationStateContainer stateContainer,
        ILogger<GodsDatabaseService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _stateContainer = stateContainer ?? throw new ArgumentNullException(nameof(stateContainer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Configures the headers dynamically based on whether we are connecting to a local PostgREST,
    /// a production Supabase instance, or an Azure SQL Data API Builder endpoint.
    /// </summary>
    private void PrepareHttpClientHeaders()
    {
        _httpClient.DefaultRequestHeaders.Remove("X-MS-API-ROLE");
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Remove("apikey");
        _httpClient.DefaultRequestHeaders.Remove("Prefer");

        // 1. Handle Supabase / standard PostgreSQL REST interface headers
        if (_stateContainer.UseSupabase && !string.IsNullOrWhiteSpace(_stateContainer.SupabaseAnonKey))
        {
            _httpClient.DefaultRequestHeaders.Add("apikey", _stateContainer.SupabaseAnonKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_stateContainer.SupabaseAnonKey}");
        }
        // 2. Handle Azure SQL REST Data API Builder (DAB) / key vault retrieved token configurations
        else
        {
            if (!string.IsNullOrWhiteSpace(_stateContainer.SupabaseAnonKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_stateContainer.SupabaseAnonKey}");
            }
            
            // DAB authorization defaults to "anonymous" for public sandbox read/write rules
            _httpClient.DefaultRequestHeaders.Add("X-MS-API-ROLE", "anonymous");
        }
    }

    /// <summary>
    /// Resolves the request URL path based on configuration host variables.
    /// </summary>
    private string GetRequestUrl(string path)
    {
        var host = _stateContainer.PostgresHost.Replace("http://", "").Replace("https://", "").TrimEnd('/');
        
        // 1. Supabase routing pattern
        if (_stateContainer.UseSupabase)
        {
            if (host.EndsWith("/rest/v1", StringComparison.OrdinalIgnoreCase))
            {
                host = host.Substring(0, host.Length - 8).TrimEnd('/');
            }
            return $"https://{host}/rest/v1/{path}";
        }
        
        // 2. Local dev container fallback (port 3000 PostgREST / DAB)
        if (host.Contains("localhost") || host.Contains("127.0.0.1"))
        {
            return $"http://{host}:3000/{path}";
        }
        
        // 3. Azure SQL DAB Production API endpoint
        return $"https://{host}/api/{path}";
    }

    /// <inheritdoc />
    public async Task<List<GodsEventCard>> FetchAllEventsAsync(CancellationToken cancellationToken = default)
    {
        // Fallback instantly if database sync is toggled off in Integrations Center
        if (!_stateContainer.IsPostgresConnected)
        {
            _logger.LogInformation("[Database Sync] Sandbox mode is active. Loading cards from local cache.");
            _stateContainer.DidLastFetchSucceed = true;
            return _stateContainer.Cards;
        }

        try
        {
            PrepareHttpClientHeaders();
            var requestUrl = GetRequestUrl("events");
            _logger.LogInformation("[Database Sync] Fetching events from database endpoint at {RequestUrl}", requestUrl);

            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var events = await response.Content.ReadFromJsonAsync<List<GodsEventCard>>(cancellationToken: cancellationToken);
                if (events != null)
                {
                    _stateContainer.Cards = events;
                    _stateContainer.DidLastFetchSucceed = true;
                    return events;
                }
            }
            _stateContainer.DidLastFetchSucceed = false;
        }
        catch (Exception ex)
        {
            _stateContainer.DidLastFetchSucceed = false;
            _logger.LogWarning(ex, "[Database Sync] Database host is currently unreachable. Falling back to local in-memory sandbox.");
        }

        return _stateContainer.Cards;
    }

    /// <inheritdoc />
    public async Task SaveEventAsync(GodsEventCard card, CancellationToken cancellationToken = default)
    {
        if (card == null) throw new ArgumentNullException(nameof(card));

        // Always persist to local active cache to ensure optimistic UI updates render instantly
        var existing = _stateContainer.Cards.FirstOrDefault(c => c.GraphEventId == card.GraphEventId);
        if (existing != null)
        {
            _stateContainer.Cards.Remove(existing);
        }
        _stateContainer.Cards.Add(card);
        _stateContainer.NotifyStateChanged();

        if (!_stateContainer.IsPostgresConnected)
        {
            _logger.LogInformation("[Database Sync] Booking gemt lokalt (sandkasse-tilstand).");
            return;
        }

        try
        {
            PrepareHttpClientHeaders();
            var requestUrl = GetRequestUrl("events");
            _logger.LogInformation("[Database Sync] Upserting event to database at {RequestUrl}", requestUrl);

            // Add PostgREST merge duplicate preference to perform upsert transparently
            if (_stateContainer.PostgresHost.Contains("localhost") || _stateContainer.UseSupabase)
            {
                _httpClient.DefaultRequestHeaders.Add("Prefer", "resolution=merge-duplicates");
            }

            using var response = await _httpClient.PostAsJsonAsync(requestUrl, card, cancellationToken);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("[Database Sync] Lykkedes at gemme '{Subject}' i databasen.", card.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Database Sync] Fejl under synkronisering til databasen. Gemt lokalt i sandkasse.");
        }
    }

    /// <inheritdoc />
    public async Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(graphEventId)) throw new ArgumentException("Identifier cannot be empty", nameof(graphEventId));

        var existing = _stateContainer.Cards.FirstOrDefault(c => c.GraphEventId == graphEventId);
        if (existing != null)
        {
            _stateContainer.Cards.Remove(existing);
        }
        _stateContainer.NotifyStateChanged();

        if (!_stateContainer.IsPostgresConnected)
        {
            _logger.LogInformation("[Database Sync] Event slettet lokalt i sandkasse.");
            return;
        }

        try
        {
            PrepareHttpClientHeaders();
            
            // Standard DAB URL style: events/id/{guid}
            var requestUrl = GetRequestUrl($"events/id/{graphEventId}");
            
            // Local PostgREST or Supabase filter style: events?id=eq.{guid}
            if (_stateContainer.PostgresHost.Contains("localhost") || _stateContainer.UseSupabase)
            {
                requestUrl = GetRequestUrl($"events?id=eq.{graphEventId}");
            }

            _logger.LogInformation("[Database Sync] Deleting event from database at {RequestUrl}", requestUrl);

            using var response = await _httpClient.DeleteAsync(requestUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("[Database Sync] Sletning af {EventId} lykkedes.", graphEventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Database Sync] Kunne ikke slette event fra databasen. Fjernet lokalt.");
        }
    }

    /// <inheritdoc />
    public async Task SyncCardPositionAsync(
        string graphEventId, 
        string sectionKey, 
        DateTimeOffset start, 
        DateTimeOffset end, 
        CancellationToken cancellationToken = default)
    {
        var card = _stateContainer.Cards.FirstOrDefault(c => c.GraphEventId == graphEventId);
        if (card != null)
        {
            card.SectionKey = sectionKey;
            card.StartUtc = start;
            card.EndUtc = end;
            _stateContainer.NotifyStateChanged();
        }

        if (!_stateContainer.IsPostgresConnected || card == null)
        {
            return;
        }

        try
        {
            PrepareHttpClientHeaders();
            
            var requestUrl = GetRequestUrl($"events/id/{graphEventId}");
            
            if (_stateContainer.PostgresHost.Contains("localhost") || _stateContainer.UseSupabase)
            {
                requestUrl = GetRequestUrl($"events?id=eq.{graphEventId}");
            }

            _logger.LogInformation("[Database Sync] Patching position on database at {RequestUrl}", requestUrl);

            using var response = await _httpClient.PatchAsJsonAsync(requestUrl, new
            {
                section_key = sectionKey,
                start_utc = start,
                end_utc = end
            }, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Database Sync] Kunne ikke opdatere selskabets position i databasen.");
        }
    }

    /// <inheritdoc />
    public async Task SeedDemoDataToDbAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTimeOffset.UtcNow.Date;
        _stateContainer.Cards.Clear();

        var seededCards = new List<GodsEventCard>
        {
            new()
            {
                GraphEventId = "demo-event-001",
                MetadataId = Guid.NewGuid(),
                Subject = "Kensington Bryllup i Den Store Lade",
                BodyContent = "Et eksklusivt gods-bryllup med fuld forplejning, blomsterdekorationer i Den Store Lade og velkomstchampagne i Søparken.",
                ColorHex = "#D4AF37",
                Priority = "High",
                SectionKey = "confirmed",
                StartUtc = today.AddHours(11),
                EndUtc = today.AddHours(18),
                EventSubtype = "Bryllup",
                GuestCount = 120,
                AssignedCoordinator = "Sarah Jenkins",
                EstateArea = "Den Store Lade",
                CateringOption = "Gourmet Selskabsmenu",
                EconomicInvoiceNumber = 104052,
                EconomicPaymentLink = "https://payment.e-conomic.com/invoice/104052/pay?token=demo_token_johnson",
                SubEvents = [
                    new() { Title = "Velkomstreception & Champagne", StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(12, 30, 0), Location = "Søparken" },
                    new() { Title = "Bryllupsmiddag & Taler", StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(17, 0, 0), Location = "Den Store Lade" },
                    new() { Title = "Brudevals & Kageskæring", StartTime = new TimeSpan(17, 15, 0), EndTime = new TimeSpan(18, 0, 0), Location = "Den Store Lade" }
                ]
            },
            new()
            {
                GraphEventId = "demo-event-002",
                MetadataId = Guid.NewGuid(),
                Subject = "Sterling Konference i Hovedbygningen",
                BodyContent = "Dagsmøde og konference i Hovedbygningen for Manor Holdings. Kræver AV-opsætning og konference-dagsmenu.",
                ColorHex = "#10B981",
                Priority = "Normal",
                SectionKey = "preparation",
                StartUtc = today.AddHours(8),
                EndUtc = today.AddHours(14),
                EventSubtype = "Konference",
                GuestCount = 80,
                AssignedCoordinator = "Michael Chang",
                EstateArea = "Hovedbygningen",
                CateringOption = "Konference-dagsmenu",
                SubEvents = [
                    new() { Title = "Morgenmad & Netværk", StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 0, 0), Location = "Hovedbygningen" },
                    new() { Title = "Formiddagssession & Keynote", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(12, 0, 0), Location = "Hovedbygningen" },
                    new() { Title = "Forretningsfrokost", StartTime = new TimeSpan(12, 0, 0), EndTime = new TimeSpan(13, 0, 0), Location = "Hovedbygningen" }
                ]
            },
            new()
            {
                GraphEventId = "demo-event-003",
                MetadataId = Guid.NewGuid(),
                Subject = "Midsommerfest i Søparken Henvendelse",
                BodyContent = "Forespørgsel på leje af herregårdshaven (Søparken) og Hovedbygningen. Kræver formelt tilbud med pakkeopstilling og kapacitetstjek.",
                ColorHex = "#D4AF37",
                Priority = "High",
                SectionKey = "inquiry",
                StartUtc = today.AddDays(2).AddHours(9),
                EndUtc = today.AddDays(2).AddHours(15),
                EventSubtype = "Bryllup",
                GuestCount = 200,
                AssignedCoordinator = "Sarah Jenkins",
                EstateArea = "Søparken",
                CateringOption = "Brunch & Champagne",
                SubEvents = [
                    new() { Title = "Velkomst & Kaffe", StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(11, 0, 0), Location = "Søparken" },
                    new() { Title = "Have-reception & Champagne", StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(14, 0, 0), Location = "Søparken" },
                    new() { Title = "Brunch & Networking", StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(16, 0, 0), Location = "Søparken" }
                ]
            }
        };

        foreach (var card in seededCards)
        {
            await SaveEventAsync(card, cancellationToken);
        }
    }
}
