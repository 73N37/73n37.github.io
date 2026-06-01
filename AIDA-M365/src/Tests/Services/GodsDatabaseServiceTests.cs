using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;
using AIDA.M365.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using Xunit;

namespace AIDA.M365.Tests.Services;

/// <summary>
/// Unit tests verifying database serialization rules, anonymous API authorizations, and cache updates.
/// </summary>
public class GodsDatabaseServiceTests
{
    private readonly Mock<ILogger<GodsDatabaseService>> _mockLogger;
    private readonly Mock<IJSRuntime> _mockJsRuntime;
    private readonly IntegrationStateContainer _stateContainer;

    public GodsDatabaseServiceTests()
    {
        _mockLogger = new Mock<ILogger<GodsDatabaseService>>();
        _mockJsRuntime = new Mock<IJSRuntime>();
        _stateContainer = new IntegrationStateContainer(_mockJsRuntime.Object);
        
        // Configures database synchronization variables to active local container sync mode by default
        _stateContainer.IsPostgresConnected = true;
        _stateContainer.PostgresHost = "localhost";
        _stateContainer.UseSupabase = false;
    }

    private HttpClient CreateMockHttpClient(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var mockMessageHandler = new MockHttpMessageHandler(handler);
        return new HttpClient(mockMessageHandler)
        {
            BaseAddress = new Uri("http://localhost:3000/")
        };
    }

    [Fact]
    public async Task FetchAllEventsAsync_ShouldUpdateLocalCache_WhenApiCallIsSuccessful()
    {
        // Arrange
        var testCard = new GodsEventCard
        {
            GraphEventId = "event-123",
            Subject = "Test Integration Wedding",
            SectionKey = "inquiry",
            StartUtc = DateTimeOffset.UtcNow,
            EndUtc = DateTimeOffset.UtcNow.AddHours(4),
            GuestCount = 100
        };

        var responseList = new List<GodsEventCard> { testCard };

        var httpClient = CreateMockHttpClient(request =>
        {
            request.RequestUri!.ToString().Should().Contain("/events");
            request.Method.Should().Be(HttpMethod.Get);
            request.Headers.GetValues("X-MS-API-ROLE").First().Should().Be("anonymous");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(responseList)
            };
        });

        var service = new GodsDatabaseService(httpClient, _stateContainer, _mockLogger.Object);

        // Act
        var result = await service.FetchAllEventsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().GraphEventId.Should().Be("event-123");
        _stateContainer.Cards.Should().HaveCount(1);
        _stateContainer.Cards.First().Subject.Should().Be("Test Integration Wedding");
        _stateContainer.DidLastFetchSucceed.Should().BeTrue();
    }

    [Fact]
    public async Task SaveEventAsync_ShouldUpdateLocalCacheAndPostToDatabase()
    {
        // Arrange
        var testCard = new GodsEventCard
        {
            GraphEventId = "event-456",
            Subject = "Catering Conference",
            SectionKey = "confirmed",
            StartUtc = DateTimeOffset.UtcNow,
            EndUtc = DateTimeOffset.UtcNow.AddHours(2),
            GuestCount = 50
        };

        var httpClient = CreateMockHttpClient(request =>
        {
            request.RequestUri!.ToString().Should().Contain("/events");
            request.Method.Should().Be(HttpMethod.Post);
            
            // Check that the request header has DAB anonymous access roles
            request.Headers.GetValues("X-MS-API-ROLE").First().Should().Be("anonymous");

            return new HttpResponseMessage(HttpStatusCode.Created);
        });

        var service = new GodsDatabaseService(httpClient, _stateContainer, _mockLogger.Object);

        // Act
        await service.SaveEventAsync(testCard);

        // Assert
        _stateContainer.Cards.Should().Contain(c => c.GraphEventId == "event-456");
        _stateContainer.Cards.First(c => c.GraphEventId == "event-456").Subject.Should().Be("Catering Conference");
    }

    [Fact]
    public async Task DeleteEventAsync_ShouldRemoveFromLocalCacheAndCallDeleteEndpoint()
    {
        // Arrange
        var testCard = new GodsEventCard
        {
            GraphEventId = "event-789",
            Subject = "Unconfirmed Party",
            SectionKey = "inquiry",
            StartUtc = DateTimeOffset.UtcNow,
            EndUtc = DateTimeOffset.UtcNow.AddHours(6),
            GuestCount = 150
        };
        
        _stateContainer.Cards.Add(testCard);

        var httpClient = CreateMockHttpClient(request =>
        {
            request.RequestUri!.ToString().Should().Contain("id=eq.event-789");
            request.Method.Should().Be(HttpMethod.Delete);

            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        var service = new GodsDatabaseService(httpClient, _stateContainer, _mockLogger.Object);

        // Act
        await service.DeleteEventAsync("event-789");

        // Assert
        _stateContainer.Cards.Should().NotContain(c => c.GraphEventId == "event-789");
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
