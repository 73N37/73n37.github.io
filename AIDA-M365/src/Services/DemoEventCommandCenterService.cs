using AIDA.M365.Models;
using Microsoft.Extensions.Logging;

namespace AIDA.M365.Services;

public sealed class DemoEventCommandCenterService : IEventCommandCenterService
{
    private readonly IOutlookCalendarEventService _outlookCalendarEventService;
    private readonly IEconomicErpService _economicErpService;
    private readonly ILogger<DemoEventCommandCenterService> _logger;

    public DemoEventCommandCenterService(
        IOutlookCalendarEventService outlookCalendarEventService,
        IEconomicErpService economicErpService,
        ILogger<DemoEventCommandCenterService> logger)
    {
        _outlookCalendarEventService = outlookCalendarEventService;
        _economicErpService = economicErpService;
        _logger = logger;
    }

    public async Task<MoveCardResult> MoveCardAsync(
        MoveCardRequest request,
        CancellationToken cancellationToken = default)
    {
        // If dragging to Confirmed, execute the 100% complete e-conomic API integration flow
        if (string.Equals(request.TargetSectionKey, "confirmed", StringComparison.OrdinalIgnoreCase))
        {
            var customer = await _economicErpService.CreateOrGetCustomerAsync(
                request.Card.AssignedCoordinator ?? "Godset Gæst",
                "reservations@engestofte.dk",
                cancellationToken);

            decimal basePrice = 18000.00m;
            if (string.Equals(request.Card.EventSubtype, "Wedding", StringComparison.OrdinalIgnoreCase) || string.Equals(request.Card.EventSubtype, "Bryllup", StringComparison.OrdinalIgnoreCase))
            {
                basePrice = 35000.00m;
            }
            else if (string.Equals(request.Card.EventSubtype, "Conference", StringComparison.OrdinalIgnoreCase) || string.Equals(request.Card.EventSubtype, "Konference", StringComparison.OrdinalIgnoreCase))
            {
                basePrice = 15000.00m;
            }
            else if (string.Equals(request.Card.EventSubtype, "HuntEvent", StringComparison.OrdinalIgnoreCase) || string.Equals(request.Card.EventSubtype, "Jagt & Event", StringComparison.OrdinalIgnoreCase))
            {
                basePrice = 25000.00m;
            }

            var lines = new List<InvoiceLineItem>
            {
                new InvoiceLineItem
                {
                    ProductNumber = "PROD-001",
                    Description = $"{request.Card.EventSubtype} Pakke - {request.Card.EstateArea}",
                    Quantity = 1.0,
                    UnitNetPrice = basePrice
                }
            };

            if (!string.IsNullOrWhiteSpace(request.Card.CateringOption))
            {
                decimal cateringPrice = 450.00m;
                if (string.Equals(request.Card.CateringOption, "Fine Dining", StringComparison.OrdinalIgnoreCase) || string.Equals(request.Card.CateringOption, "Gourmet Selskabsmenu", StringComparison.OrdinalIgnoreCase))
                {
                    cateringPrice = 1250.00m;
                }
                else if (string.Equals(request.Card.CateringOption, "Konference-dagsmenu", StringComparison.OrdinalIgnoreCase))
                {
                    cateringPrice = 650.00m;
                }
                else if (string.Equals(request.Card.CateringOption, "Brunch & Champagne", StringComparison.OrdinalIgnoreCase))
                {
                    cateringPrice = 850.00m;
                }

                lines.Add(new InvoiceLineItem
                {
                    ProductNumber = "PROD-002",
                    Description = $"{request.Card.CateringOption} for {request.Card.GuestCount} gæster",
                    Quantity = request.Card.GuestCount,
                    UnitNetPrice = cateringPrice
                });
            }

            if (!string.IsNullOrWhiteSpace(request.Card.AssignedCoordinator))
            {
                lines.Add(new InvoiceLineItem
                {
                    ProductNumber = "PROD-003",
                    Description = "Koordinator- & AV-personale service",
                    Quantity = 1.0,
                    UnitNetPrice = 4500.00m
                });
            }

            var draft = await _economicErpService.CreateDraftInvoiceAsync(customer.CustomerNumber, lines, cancellationToken);
            var booked = await _economicErpService.BookInvoiceAsync(draft.DraftInvoiceNumber, cancellationToken);

            request.Card.EconomicInvoiceNumber = booked.BookedInvoiceNumber;
            request.Card.EconomicPaymentLink = booked.PaymentLink;
        }

        await _outlookCalendarEventService.UpdateEventTimeAsync(
            request.Card.GraphEventId,
            request.StartUtc,
            request.EndUtc,
            cancellationToken);

        _logger.LogInformation(
            "[DEMO] Simulated metadata save for event {EventId}",
            request.Card.GraphEventId);

        return MoveCardResult.Success();
    }

    public Task<CardSummaryResult> SummarizeCardAsync(
        KanbanEventCard card,
        CancellationToken cancellationToken = default)
    {
        string summary;
        string[] actions;

        if (string.Equals(card.EventSubtype, "Wedding", StringComparison.OrdinalIgnoreCase) || string.Equals(card.EventSubtype, "Bryllup", StringComparison.OrdinalIgnoreCase))
        {
            summary = $"AI Gods-Assistent (Azure OpenAI GPT-4o): Høj-prioritets bryllup på Engestofte Gods. Kunden har reserveret {card.EstateArea ?? "Den Store Lade"} til {card.GuestCount} gæster. Koordinatoren {card.AssignedCoordinator ?? "afventer tildeling"} skal koordinere bordopstilling, gourmetmenu og tidsplan.";
            
            actions =
            [
                $"Bekræft blomsterdekorationer og adgangstilladelse til {card.EstateArea ?? "Den Store Lade"}",
                $"Opsæt bordplan til {card.GuestCount} gæster inklusiv dansegulv",
                $"Gennemse detaljer for forplejningen '{card.CateringOption ?? "Gourmet Selskabsmenu"}'",
                $"Planlæg generalprøve med koordinator {card.AssignedCoordinator ?? "afventer tildeling"}"
            ];
        }
        else if (string.Equals(card.EventSubtype, "Conference", StringComparison.OrdinalIgnoreCase) || string.Equals(card.EventSubtype, "Konference", StringComparison.OrdinalIgnoreCase))
        {
            summary = $"AI Gods-Assistent (Azure OpenAI GPT-4o): Konference booket i {card.EstateArea ?? "Hovedbygningen"}. Forventet deltagelse er {card.GuestCount} personer. AV-opsætning og pauseforplejning er markeret i drejebogen.";
            
            actions =
            [
                $"Klargør projektor, lærred og mikrofoner i {card.EstateArea ?? "Hovedbygningen"}",
                $"Koordiner servering af {card.CateringOption ?? "Konference-dagsmenu"} til pauserne",
                $"Sikre tilstrækkeligt internetbåndbredde til {card.GuestCount} deltagere",
                $"Koordinator {card.AssignedCoordinator ?? "afventer tildeling"} byder velkommen ved Hovedbygningen"
            ];
        }
        else
        {
            summary = $"AI Gods-Assistent (Azure OpenAI GPT-4o): Arrangement i {card.EstateArea ?? "Søparken"}. Gæsteantal: {card.GuestCount}. Detaljer og catering er klargjort.";
            
            actions =
            [
                $"Koordiner udendørs opsætning i {card.EstateArea ?? "Søparken"}",
                $"Klargør serverings checkliste for {card.CateringOption ?? "Brunch & Champagne"}",
                $"Briefing med serveringspersonale ved koordinator {card.AssignedCoordinator ?? "afventer tildeling"}",
                $"Opsæt velkomstskilt ved godsets indgangsport"
            ];
        }

        return Task.FromResult(CardSummaryResult.Success(summary, actions));
    }
}
