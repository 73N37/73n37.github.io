# AIDA-M365

Starter implementation for a Blazor + MudBlazor Kanban board where each card maps to an Outlook calendar event and drag-drop updates event time through Microsoft Graph.

## Added Components

- `src/Components/KanbanBoard.razor`: reusable MudBlazor drag-drop board.
- `src/Components/KanbanBoardDemo.razor`: parent page that seeds sample sections and cards.
- `src/Models/KanbanEventCard.cs`: card model with Graph event id and UTC time values.
- `src/Models/KanbanSectionDefinition.cs`: destination column timing logic.
- `src/Services/IOutlookCalendarEventService.cs`: service contract for event updates.
- `src/Services/OutlookCalendarEventService.cs`: GraphServiceClient PATCH implementation.
- `src/Services/DemoOutlookCalendarEventService.cs`: no-op logging service for local demo runs.
- `src/Extensions/ServiceCollectionExtensions.cs`: registers Graph-backed service.
- `src/Extensions/DemoServiceCollectionExtensions.cs`: registers demo service.

## Dependency Injection Setup

In your Blazor app `Program.cs`, register one of these modes:

### 1) Local Demo Mode (no Graph call)

```csharp
using AIDA.M365.Extensions;

builder.Services.AddAidaKanbanDemoServices();
```

### 2) Live Graph Mode (real Outlook update)

```csharp
using AIDA.M365.Extensions;

// GraphServiceClient should already be configured with Microsoft.Identity.Web.
builder.Services.AddAidaKanbanServices();
```

## Step-By-Step Test Guide

### A. Verify Drag-Drop End-to-End Locally

1. Ensure MudBlazor services are registered and Mud providers are present in your app layout.
2. Register demo mode service in `Program.cs`:
	- `builder.Services.AddAidaKanbanDemoServices();`
3. Run the Blazor app.
4. Open route `/kanban-demo`.
5. Drag one card from any column to another.
6. Confirm results:
	- Card appears in the target column.
	- Success alert updates with new section and recalculated UTC time range.
	- App remains responsive with no exception page.
7. Optional verification in logs:
	- Look for `[DEMO] Simulated Graph PATCH for event ...`.

### B. Verify Real Outlook Update Through Graph

1. Switch DI registration to:
	- `builder.Services.AddAidaKanbanServices();`
2. Confirm your Graph auth setup is valid:
	- Signed-in user has calendar permission for event write (for example `Calendars.ReadWrite`).
	- `GraphServiceClient` resolves from DI.
3. Seed cards with real `GraphEventId` values from the signed-in user's calendar.
4. Run the app and open `/kanban-demo`.
5. Drag a card to another column.
6. Confirm results:
	- Card moves in the UI.
	- No thrown exception from `ItemUpdated`.
	- Outlook event start/end are updated to the target section schedule.
7. Open Outlook calendar and verify the moved event has the new times.

## Notes

- Time recalculation uses UTC and preserves original event duration.
- On Graph update failure, local card state is rolled back to original values.
- `CardUpdated` callback on the parent component can be used to trigger additional UI or persistence actions.