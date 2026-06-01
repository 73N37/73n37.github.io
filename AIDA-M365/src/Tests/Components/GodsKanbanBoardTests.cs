using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Components;
using AIDA.M365.Models;
using AIDA.M365.Services;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using MudBlazor.Interop;
using Xunit;

namespace AIDA.M365.Tests.Components;

/// <summary>
/// Component tests verifying the visual layout, drop zone counts, and columns inside GodsKanbanBoard.
/// </summary>
public class GodsKanbanBoardTests : TestContext
{
    public GodsKanbanBoardTests()
    {
        // Register MudBlazor styling services in the bUnit context
        Services.AddMudServices();
        
        // Mock Command Center Service operations
        var mockService = new Mock<IGodsCommandCenterService>();
        mockService
            .Setup(x => x.MoveCardAsync(It.IsAny<EventCardMoveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EventCardMoveResult.Success());
        mockService
            .Setup(x => x.SummarizeCardAsync(It.IsAny<GodsEventCard>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EventCardSummaryResult.Success("summary", ["action"]));
        Services.AddSingleton(mockService.Object);
        
        var mockLogger = new Mock<ILogger<GodsKanbanBoard>>();
        Services.AddSingleton(mockLogger.Object);

        // Emulates JSInterop drag-and-drop registrations used internally by MudBlazor
        JSInterop.SetupVoid("mudDragAndDrop.initDropZone", _ => true);
        JSInterop.SetupVoid("mudDragAndDrop.initContainer", _ => true);
        JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
    }

    [Fact]
    public void GodsKanbanBoard_ShouldRenderSections()
    {
        // Arrange
        var sections = new List<GodsKanbanSection>
        {
            new() { Key = "todo", Title = "To Do", DayOffsetFromTodayUtc = 0, StartHourUtc = 9 },
            new() { Key = "done", Title = "Done", DayOffsetFromTodayUtc = 1, StartHourUtc = 9 }
        };
        var cards = new List<GodsEventCard>();

        // Act
        var cut = RenderComponent<GodsKanbanBoard>(parameters => parameters
            .Add(p => p.Sections, sections)
            .Add(p => p.Cards, cards)
        );

        // Assert
        cut.FindAll(".mud-paper").Count.Should().BeGreaterThan(0);
        foreach (var section in sections)
        {
            cut.Markup.Should().Contain(section.Title);
        }
    }
}
