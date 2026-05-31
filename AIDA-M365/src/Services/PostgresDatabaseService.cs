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

public sealed class PostgresDatabaseService : IPostgresDatabaseService
{
    private readonly HttpClient _httpClient;
    private readonly IntegrationStateContainer _stateContainer;
    private readonly ILogger<PostgresDatabaseService> _logger;

    public PostgresDatabaseService(
        HttpClient httpClient,
        IntegrationStateContainer stateContainer,
        ILogger<PostgresDatabaseService> logger)
    {
        _httpClient = httpClient;
        _stateContainer = stateContainer;
        _logger = logger;
    }

    private void PrepareHttpClientHeaders()
    {
        _httpClient.DefaultRequestHeaders.Remove("apikey");
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Remove("Prefer");

        if (_stateContainer.UseSupabase && !string.IsNullOrWhiteSpace(_stateContainer.SupabaseAnonKey))
        {
            _httpClient.DefaultRequestHeaders.Add("apikey", _stateContainer.SupabaseAnonKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_stateContainer.SupabaseAnonKey}");
        }
    }

    private string GetRequestUrl(string path)
    {
        if (_stateContainer.UseSupabase)
        {
            var host = _stateContainer.PostgresHost.Replace("http://", "").Replace("https://", "").TrimEnd('/');
            if (host.EndsWith("/rest/v1", StringComparison.OrdinalIgnoreCase))
            {
                host = host.Substring(0, host.Length - 8).TrimEnd('/');
            }
            return $"https://{host}/rest/v1/{path}";
        }
        else
        {
            return $"http://{_stateContainer.PostgresHost}:3000/{path}";
        }
    }

    public async Task<List<KanbanEventCard>> FetchAllEventsAsync(CancellationToken cancellationToken = default)
    {
        if (!_stateContainer.IsPostgresConnected)
        {
            _logger.LogInformation("[PostgreSQL] Backup sync is offline. Fetching from local in-memory state.");
            _stateContainer.DidLastFetchSucceed = true;
            return _stateContainer.Cards;
        }

        try
        {
            PrepareHttpClientHeaders();
            var requestUrl = GetRequestUrl("events");
            _logger.LogInformation("[PostgreSQL] Fetching events from database at {RequestUrl}", requestUrl);

            // Attempt to fetch from real API REST endpoint
            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var events = await response.Content.ReadFromJsonAsync<List<KanbanEventCard>>(cancellationToken: cancellationToken);
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
            _logger.LogWarning(ex, "[PostgreSQL] Host is currently offline or unreachable. Falling back to local cache.");
        }

        return _stateContainer.Cards;
    }

    public async Task SaveEventAsync(KanbanEventCard card, CancellationToken cancellationToken = default)
    {
        // Always persist to local active cache
        var existing = _stateContainer.Cards.FirstOrDefault(c => c.GraphEventId == card.GraphEventId);
        if (existing != null)
        {
            _stateContainer.Cards.Remove(existing);
        }
        _stateContainer.Cards.Add(card);
        _stateContainer.NotifyStateChanged();

        if (!_stateContainer.IsPostgresConnected)
        {
            _logger.LogInformation("[PostgreSQL] Backup synkronisering er inaktiv. Gemt lokalt.");
            return;
        }

        try
        {
            PrepareHttpClientHeaders();
            if (_stateContainer.UseSupabase)
            {
                _httpClient.DefaultRequestHeaders.Add("Prefer", "resolution=merge-duplicates");
            }
            var requestUrl = GetRequestUrl("events");
            _logger.LogInformation("[PostgreSQL] Upserting event to database at {RequestUrl}", requestUrl);

            using var response = await _httpClient.PostAsJsonAsync(requestUrl, card, cancellationToken);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("[PostgreSQL] Synkronisering af '{Subject}' til PostgreSQL lykkedes.", card.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PostgreSQL] Synkronisering mislykkedes til databasen. Gemt lokalt i sandkasse.");
        }
        finally
        {
            if (_stateContainer.UseSupabase)
            {
                _httpClient.DefaultRequestHeaders.Remove("Prefer");
            }
        }
    }

    public async Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default)
    {
        var existing = _stateContainer.Cards.FirstOrDefault(c => c.GraphEventId == graphEventId);
        if (existing != null)
        {
            _stateContainer.Cards.Remove(existing);
        }
        _stateContainer.NotifyStateChanged();

        if (!_stateContainer.IsPostgresConnected)
        {
            _logger.LogInformation("[PostgreSQL] Slettet lokalt. PostgreSQL synkronisering offline.");
            return;
        }

        try
        {
            PrepareHttpClientHeaders();
            var requestUrl = GetRequestUrl($"events?id=eq.{graphEventId}");
            _logger.LogInformation("[PostgreSQL] Deleting event from database at {RequestUrl}", requestUrl);

            using var response = await _httpClient.DeleteAsync(requestUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("[PostgreSQL] Sletning af {EventId} i PostgreSQL lykkedes.", graphEventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PostgreSQL] Kunne ikke slette på database. Fjernet lokalt.");
        }
    }

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
            var requestUrl = GetRequestUrl($"events?id=eq.{graphEventId}");
            _logger.LogInformation("[PostgreSQL] Updating section position on database at {RequestUrl}", requestUrl);

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
            _logger.LogError(ex, "[PostgreSQL] Kunne ikke opdatere position på database.");
        }
    }

    public Task SeedDemoDataToDbAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTimeOffset.UtcNow.Date;
        _stateContainer.Cards.Clear();

        var seededCards = new List<KanbanEventCard>
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
                Subevents = [
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
                Subevents = [
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
                Subevents = [
                    new() { Title = "Velkomst & Kaffe", StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(11, 0, 0), Location = "Søparken" },
                    new() { Title = "Have-reception & Champagne", StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(14, 0, 0), Location = "Søparken" },
                    new() { Title = "Brunch & Networking", StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(16, 0, 0), Location = "Søparken" }
                ]
            }
        };

        _stateContainer.Cards.AddRange(seededCards);
        _stateContainer.NotifyStateChanged();
        return Task.CompletedTask;
    }
}
