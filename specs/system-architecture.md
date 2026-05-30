# Specification: System Architecture

## 1. Technical Framework
The Godset Estate Venue rental platform is structured as a decoupled solution:
- **Frontend**: Blazor WebAssembly running MudBlazor modern components (cream/slate/gold palette).
- **Backend API**: ASP.NET Core REST API serving as a secure BFF (Backend-For-Frontend).
- **Caching & Persistence**: Azure SQL Database storing custom event metadata (seating sketches, checklists, state, visual categories) and change logs.
- **Security Key Management**: Azure Key Vault holding API tokens and M365 client credentials.

## 2. API Gateways & Service Sync
```
[Blazor WASM UI] <---(HTTPS/Cookies)---> [BFF ASP.NET Core API]
                                            |--> Azure Key Vault (Credentials)
                                            |--> Azure SQL (Metadata cache)
                                            |--> Microsoft Graph & Bookings API
                                            |--> Dynamics 365 / Dataverse API
                                            |--> Azure OpenAI (GPT-4o)
                                            |--> e-conomic REST ERP API
```

## 3. Bi-Directional Event Synchronization
1. **Outbound Sync**: Dragging cards on the Kanban board dispatches asynchronous orchestrations updating Outlook events in Graph, Leads in CRM, and Invoices in e-conomic.
2. **Inbound Sync**: Webhook subscriptions on Microsoft Graph listen for client modifications, pushing changes instantly to the Azure SQL cache and client-side Blazor UI via SignalR.
3. **Rollback Policy**: All outbound API sync blocks are executed sequentially. If any API call fails (such as an e-conomic validation exception or Graph network timeout), the transaction is aborted, the local state rolls back to its original value, and a warning is dispatched via Blazor's Snackbar.
