Act as a Senior .NET Architect. I am building a "Strategic Event Command Center" using Blazor WebAssembly and MudBlazor to replace Trello. This app must orchestrate data across the Microsoft 365 stack.

Project Architecture & Context:
1. UI Framework: MudBlazor using MudDropContainer for a Kanban board. Columns represent Outlook/Dynamics event categories.
2. Identity: Integrated via Microsoft.Identity.Web (Entra ID) with Scopes for Graph (Calendars.ReadWrite) and Dynamics (user_impersonation).
3. Data Sources (The "Hub"): 
   - Primary: Microsoft Graph API (Outlook Events).
   - Secondary: Dynamics 365 (linking Events to 'Accounts' or 'Opportunities').
   - Persistent: Azure SQL for custom metadata (e.g., card colors, user preferences, audit logs) not stored in Graph.
4. Intelligence: Azure OpenAI (GPT-4o) used to analyze event descriptions and suggest the best Kanban column/time-slot for a task.

Business Logic Requirements:
- Sync Logic: Dragging a card triggers a Graph PATCH for the time change AND a Dynamics Web API update if the event is linked to a Lead/Account.
- State Management: Implement a "Loading" overlay while the multi-service update (Graph + Dynamics) is in flight.
- AI Integration: Add a "Smart Summarize" button on the card that uses Azure OpenAI to extract action items from the Outlook Event body.

Task:
Please provide a high-level, production-ready implementation of:
- A Card Model: Incorporate Graph EventID, Dynamics EntityID, and Azure SQL MetadataID.
- The Razor Component: A MudDropContainer that handles the 'ItemUpdated' event asynchronously.
- The Orchestration Service: A C# service class that uses:
    - GraphServiceClient for Calendar updates.
    - HttpClient (with IDiscoveryService or MSAL) for Dynamics 365 Web API updates.
    - Semantic Kernel or Azure.AI.OpenAI client for the summary feature.
- Azure SQL Repository: A simple Dapper or EF Core snippet to save 'CardCustomizations'.

Ensure the architecture follows the "Repository Pattern" or "Service Layer Pattern" to keep the Blazor components lean. Use modern C# 12 features.