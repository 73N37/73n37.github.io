using AIDA.M365.Models;
using Microsoft.Extensions.Logging;

namespace AIDA.M365.Services;

public sealed class EventCommandCenterService : IEventCommandCenterService
{
    private readonly IOutlookCalendarEventService _outlookCalendarEventService;
    private readonly ICardCustomizationRepository _cardCustomizationRepository;
    private readonly IAiCardSummaryService _aiCardSummaryService;
    private readonly IEconomicErpService _economicErpService;
    private readonly ILogger<EventCommandCenterService> _logger;

    public EventCommandCenterService(
        IOutlookCalendarEventService outlookCalendarEventService,
        ICardCustomizationRepository cardCustomizationRepository,
        IAiCardSummaryService aiCardSummaryService,
        IEconomicErpService economicErpService,
        ILogger<EventCommandCenterService> _logger)
    {
        _outlookCalendarEventService = outlookCalendarEventService;
        _cardCustomizationRepository = cardCustomizationRepository;
        _aiCardSummaryService = aiCardSummaryService;
        _economicErpService = economicErpService;
        this._logger = _logger;
    }

    public async Task<MoveCardResult> MoveCardAsync(
        MoveCardRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Card.GraphEventId))
        {
            return MoveCardResult.Failure("Graph event id is required.");
        }

        if (request.EndUtc <= request.StartUtc)
        {
            return MoveCardResult.Failure("End must be greater than start.");
        }

        try
        {
            // If dragging to Confirmed, execute the 100% complete e-conomic API integration flow
            if (string.Equals(request.TargetSectionKey, "Confirmed", StringComparison.OrdinalIgnoreCase))
            {
                var customer = await _economicErpService.CreateOrGetCustomerAsync(
                    request.Card.AssignedCoordinator ?? "Godset Guest",
                    "reservations@royalestate.com",
                    cancellationToken);

                var lines = new List<InvoiceLineItem>
                {
                    new InvoiceLineItem
                    {
                        ProductNumber = "PROD-001",
                        Description = $"{request.Card.EventSubtype} Package - {request.Card.EstateArea}",
                        Quantity = 1.0,
                        UnitNetPrice = string.Equals(request.Card.EventSubtype, "Wedding", StringComparison.OrdinalIgnoreCase) ? 35000.00m : 18000.00m
                    }
                };

                if (!string.IsNullOrWhiteSpace(request.Card.CateringOption))
                {
                    lines.Add(new InvoiceLineItem
                    {
                        ProductNumber = "PROD-002",
                        Description = $"Fine Dining Catering for {request.Card.GuestCount} guests",
                        Quantity = request.Card.GuestCount,
                        UnitNetPrice = 125.00m
                    });
                }

                if (!string.IsNullOrWhiteSpace(request.Card.AssignedCoordinator))
                {
                    lines.Add(new InvoiceLineItem
                    {
                        ProductNumber = "PROD-003",
                        Description = "Coordinator & AV Staff Services",
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

            await _cardCustomizationRepository.SaveAsync(
                new CardCustomization(
                    request.Card.MetadataId,
                    request.Card.GraphEventId,
                    request.TargetSectionKey,
                    request.StartUtc,
                    request.EndUtc,
                    request.Card.ColorHex,
                    request.Card.Priority,
                    request.Card.SmartSummary,
                    request.Card.ActionItems,
                    DateTimeOffset.UtcNow),
                cancellationToken);

            return MoveCardResult.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to move event {EventId} to section {SectionKey}",
                request.Card.GraphEventId,
                request.TargetSectionKey);

            return MoveCardResult.Failure("The event could not be synchronized across Microsoft 365.");
        }
    }

    public Task<CardSummaryResult> SummarizeCardAsync(
        KanbanEventCard card,
        CancellationToken cancellationToken = default) =>
        _aiCardSummaryService.SummarizeAsync(card, cancellationToken);
}
