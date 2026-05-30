# AIDA-M365 Project Documentation

## Project Structure
This project is a Blazor-based application for managing Outlook Calendar events via a Kanban board.

- **AIDA.M365.csproj**: Main project file (Razor SDK).
- **Components/**: Blazor components (using MudBlazor).
- **Services/**: Core logic for interacting with Microsoft Graph.
- **Models/**: Data models.
- **Extensions/**: Dependency Injection setup.
- **Tests/**: xUnit test project.

## Testing Strategy
The project uses a multi-layered testing approach:

1.  **Unit Tests (xUnit & Moq)**: Used for testing services like `OutlookCalendarEventService`. We mock the `GraphServiceClient` and other dependencies to verify business logic and error handling.
2.  **Component Tests (bUnit)**: Used for testing Blazor components in `Components/`.
    *   **MudBlazor Support**: Component tests are configured to mock MudBlazor's JavaScript interop calls (e.g., drag and drop initialization).

## Running Tests
To run all tests, use the following command from the `src` directory:

```bash
dotnet test AIDA-M365.sln
```

## Known Issues / Notes
- **Razor Directives**: Avoid using `@section` as a variable name in `.razor` files as it conflicts with the built-in Razor `@section` directive.
- **Graph Mocking**: Mocking Microsoft Graph v5 can be complex; consider using a wrapper or specialized testing libraries for more comprehensive service tests.
