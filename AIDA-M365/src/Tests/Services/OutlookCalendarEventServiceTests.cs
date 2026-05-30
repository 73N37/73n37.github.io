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

public class OutlookCalendarEventServiceTests
{
    private readonly Mock<GraphServiceClient> _mockGraphClient;
    private readonly Mock<ILogger<OutlookCalendarEventService>> _mockLogger;
    private readonly OutlookCalendarEventService _service;

    public OutlookCalendarEventServiceTests()
    {
        // Note: Mocking GraphServiceClient v5 can be complex due to its fluent interface.
        // In a real scenario, we might use a wrapper or the built-in testing support if available.
        // For this example, we'll focus on the logic and assume the client structure.
        
        // This is a simplified mock setup. 
        // Real Graph v5 mocking often requires mocking the RequestBuilders.
        _mockGraphClient = new Mock<GraphServiceClient>(new Mock<Microsoft.Kiota.Abstractions.Authentication.IAuthenticationProvider>().Object, "https://graph.microsoft.com/v1.0");
        _mockLogger = new Mock<ILogger<OutlookCalendarEventService>>();
        _service = new OutlookCalendarEventService(_mockGraphClient.Object, _mockLogger.Object);
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
