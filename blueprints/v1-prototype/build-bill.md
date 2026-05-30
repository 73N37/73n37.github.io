# Build Bill of Materials (V1 Prototype)

This document specifies the exact library dependencies and external dependencies.

## 1. Frontend Blazor WebAssembly
- **MudBlazor (v6.15.0)**: Premium component library providing grid layouts, Kanban containers, dialogs, drawers, and form validation elements.
- **Microsoft.AspNetCore.Components.WebAssembly (8.0.0)**: Blazor WASM core runtime.
- **Microsoft.AspNetCore.Components.WebAssembly.DevServer (8.0.0)**: Local host development server.

## 2. Backend ASP.NET Core API
- **Microsoft.Identity.Web (2.15.0)**: Token caching, Entra ID MSAL integration, and Graph delegation.
- **Azure.Identity (1.10.0)**: Entra passwordless connectivity to Key Vault and SQL.
- **Dapper (2.1.0)**: High performance micro-ORM for caching data operations and diagnostics.
- **QuestPDF (2023.12.0)**: Fluent layout library for compiling beautiful contract PDFs on the server.
- **Azure.AI.OpenAI (1.0.0-beta.8)**: High-level SDK for dispatching prompts to GPT-4o deployments.
