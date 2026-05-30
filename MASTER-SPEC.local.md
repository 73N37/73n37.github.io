# MASTER-SPEC.local: Local Environment Configuration

This document specifies the local development guidelines and diagnostics for the **Godset Estate Scheduling & Invoicing Integration**.

## 1. Prerequisites
- **SDKs**: .NET 8.0 SDK, Python 3.10+ (for simulation and validation scripts).
- **IDEs**: Visual Studio 2022 or VS Code.
- **Dependencies**: MudBlazor UI library is wired inside Blazor WASM.

## 2. Dev Server Start
To run the Blazor WebAssembly and hosting stack locally:
```bash
cd AIDA-M365/src
dotnet run
```
- Open browser at `http://localhost:5200` or the logged dev-server port.
- Access the Integration Hub (`/connect`) to test mock diagnostic ping tests.
- Navigate to the Kanban Command Board (`/`) to test optimistic drag-and-drop operations with e-conomic integrations.
- Navigate to Upcoming Events (`/upcoming-events`) to view the consolidated overlay calendar.

## 3. Local Mocking Architecture
The system uses `AddAidaKanbanDemoServices()` by default in development mode.
- Mock Graph API dispatches simulated email acknowledgement and Graph Calendar events.
- Mock Dynamics CRM service mimics Leads/Opportunities generation.
- Mock e-conomic service (`DemoEconomicErpService`) provides draft and invoice booking loops, returning mock transaction/payment links.
