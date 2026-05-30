using AIDA.M365.Models;
using AIDA.M365.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AIDA.M365.Tests.Services;

public sealed class DynamicsEventLinkServiceTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new(MockBehavior.Strict);
    private readonly Mock<ILogger<DynamicsEventLinkService>> _loggerMock = new();
    private readonly DynamicsOptions _options = new()
    {
        EnvironmentUrl = "https://73n37.crm4.dynamics.com/",
        ApiVersion = "v9.2"
    };

    private DynamicsEventLinkService CreateService()
    {
        var httpClient = new HttpClient(_handlerMock.Object);
        var optionsMock = new Mock<IOptions<DynamicsOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);
        return new DynamicsEventLinkService(httpClient, optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task UpdateLinkedEventAsync_ShouldThrowInvalidOperationException_WhenEnvironmentUrlIsEmpty()
    {
        // Arrange
        _options.EnvironmentUrl = string.Empty;
        var service = CreateService();
        var update = new DynamicsEventUpdate(
            "account",
            "12345",
            "graph-evt-001",
            "confirmed",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(2));

        // Act
        Func<Task> act = async () => await service.UpdateLinkedEventAsync(update);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Dynamics environment URL is not configured.");
    }

    [Theory]
    [InlineData("account", "accounts")]
    [InlineData("opportunity", "opportunities")]
    [InlineData("lead", "leads")]
    [InlineData("custom_entity", "custom_entities")]
    public async Task UpdateLinkedEventAsync_ShouldSendPatchRequestToCorrectUri_ForVariousLogicalNames(
        string logicalName,
        string expectedEntitySetName)
    {
        // Arrange
        var service = CreateService();
        var entityId = "id-123";
        var graphEventId = "evt-789";
        var start = new DateTimeOffset(2026, 5, 30, 12, 0, 0, TimeSpan.Zero);
        var end = start.AddHours(2);
        var update = new DynamicsEventUpdate(logicalName, entityId, graphEventId, "confirmed", start, end);

        var expectedUri = $"https://73n37.crm4.dynamics.com/api/data/v9.2/{expectedEntitySetName}({entityId})";

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Patch &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString() == expectedUri),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();

        // Act
        await service.UpdateLinkedEventAsync(update);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Patch &&
                req.RequestUri != null &&
                req.RequestUri.ToString() == expectedUri),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task UpdateLinkedEventAsync_ShouldVerifyJsonPayloadProperties()
    {
        // Arrange
        var service = CreateService();
        var start = new DateTimeOffset(2026, 5, 30, 10, 0, 0, TimeSpan.Zero);
        var end = start.AddHours(3);
        var update = new DynamicsEventUpdate("lead", "lead-456", "graph-evt-222", "preparation", start, end);

        HttpRequestMessage? interceptedRequest = null;
        string? payload = null;

        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (req, ct) =>
            {
                interceptedRequest = req;
                if (req.Content != null)
                {
                    payload = await req.Content.ReadAsStringAsync(ct);
                }
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            })
            .Verifiable();

        // Act
        await service.UpdateLinkedEventAsync(update);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());

        interceptedRequest.Should().NotBeNull();
        interceptedRequest!.Method.Should().Be(HttpMethod.Patch);
        interceptedRequest.Content.Should().NotBeNull();

        payload.Should().NotBeNull();
        payload.Should().Contain("graph-evt-222");
        payload.Should().Contain("preparation");
    }
}
