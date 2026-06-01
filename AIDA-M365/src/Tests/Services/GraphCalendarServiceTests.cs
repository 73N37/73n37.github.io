using AIDA.M365.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIDA.M365.Tests.Services;

/// <summary>
/// Unit tests verifying calendar event start/end date range assertions and validation rules.
/// </summary>
public class GraphCalendarServiceTests
{
    private readonly Mock<GraphServiceClient> _mockGraphClient;
    private readonly Mock<ILogger<GraphCalendarService>> _mockLogger;
    private readonly GraphCalendarService _service;

    public GraphCalendarServiceTests()
    {
        _mockGraphClient = new Mock<GraphServiceClient>(new Mock<Microsoft.Kiota.Abstractions.Authentication.IAuthenticationProvider>().Object, "https://graph.microsoft.com/v1.0");
        _mockLogger = new Mock<ILogger<GraphCalendarService>>();
        _service = new GraphCalendarService(_mockGraphClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UpdateEventTimeAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.UpdateEventTimeAsync("", start, end));
    }

    [Fact]
    public async Task UpdateEventTimeAsync_ShouldThrowArgumentException_WhenEndIsBeforeStart()
    {
        // Arrange
        var start = DateTimeOffset.UtcNow;
        var end = start.AddHours(-1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.UpdateEventTimeAsync("event-id", start, end));
    }
}
