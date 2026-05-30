using AIDA.M365.Models;
using AIDA.M365.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIDA.M365.Tests.Services;

public sealed class EventCommandCenterServiceTests
{
    private readonly Mock<IOutlookCalendarEventService> _outlook = new();
    private readonly Mock<ICardCustomizationRepository> _repository = new();
    private readonly Mock<IAiCardSummaryService> _ai = new();
    private readonly Mock<IEconomicErpService> _economic = new();
    private readonly Mock<ILogger<EventCommandCenterService>> _logger = new();

    [Fact]
    public async Task MoveCardAsync_ShouldPatchGraph_WhenCardIsNotConfirmed()
    {
        var service = CreateService();
        var card = CreateCard();
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(1);

        var result = await service.MoveCardAsync(new MoveCardRequest(card, "planned", start, end));

        result.Succeeded.Should().BeTrue();
        _outlook.Verify(x => x.UpdateEventTimeAsync(card.GraphEventId, start, end, It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(x => x.SaveAsync(It.Is<CardCustomization>(c => c.GraphEventId == card.GraphEventId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MoveCardAsync_ShouldBookInvoice_WhenTargetIsConfirmed()
    {
        var service = CreateService();
        var card = CreateCard();
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(1);

        _economic.Setup(x => x.CreateOrGetCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EconomicCustomer { CustomerNumber = 12345, Name = "Sarah Jenkins", Email = "reservations@royalestate.com" });

        _economic.Setup(x => x.CreateDraftInvoiceAsync(It.IsAny<int>(), It.IsAny<List<InvoiceLineItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EconomicDraftInvoice { DraftInvoiceNumber = 987, CustomerNumber = 12345 });

        _economic.Setup(x => x.BookInvoiceAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EconomicBookedInvoice { BookedInvoiceNumber = 999111, PaymentLink = "https://pay.link" });

        var result = await service.MoveCardAsync(new MoveCardRequest(card, "Confirmed", start, end));

        result.Succeeded.Should().BeTrue();
        card.EconomicInvoiceNumber.Should().Be(999111);
        card.EconomicPaymentLink.Should().Be("https://pay.link");
        _economic.Verify(x => x.CreateOrGetCustomerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _economic.Verify(x => x.CreateDraftInvoiceAsync(It.IsAny<int>(), It.IsAny<List<InvoiceLineItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        _economic.Verify(x => x.BookInvoiceAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MoveCardAsync_ShouldReturnFailure_WhenEndIsBeforeStart()
    {
        var service = CreateService();
        var start = DateTimeOffset.UtcNow;

        var result = await service.MoveCardAsync(new MoveCardRequest(CreateCard(), "planned", start, start.AddMinutes(-1)));

        result.Succeeded.Should().BeFalse();
        _outlook.Verify(x => x.UpdateEventTimeAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private EventCommandCenterService CreateService() =>
        new(_outlook.Object, _repository.Object, _ai.Object, _economic.Object, _logger.Object);

    private static KanbanEventCard CreateCard() =>
        new()
        {
            GraphEventId = "event-1",
            Subject = "Test",
            SectionKey = "backlog",
            StartUtc = DateTimeOffset.UtcNow,
            EndUtc = DateTimeOffset.UtcNow.AddHours(1)
        };
}
