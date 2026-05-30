# Engestofte Gods Booking & Afviklingsmotor – Teknisk Dokumentation

Velkommen til den tekniske dokumentation for **Engestofte Gods Booking Engine**. Dette dokument beskriver hjemmesidens overordnede formål, de vigtigste funktionaliteter samt den bagvedliggende teknologiske stak (tech stack).

---

## 1. Formål & Forretningskontekst

Applikationen er skræddersyet til **Engestofte Gods** (Lolland, Danmark) med det formål at digitalisere, strømline og automatisere hele livscyklussen for herregårdens eksklusive arrangementer. Systemet understøtter fire selskabstyper:
*   **Bryllupper** (f.eks. i *Den Store Lade* eller *Søparken*)
*   **Konferencer & Dagsmøder** (i *Hovedbygningen*)
*   **Private Fester & Jubilæer**
*   **Jagt & Events**

Systemet fungerer som en central kommando- og styringstavle for godsets koordinatorer, men understøtter også et offentligt klient-workflow, hvor kunder kan sende reservationsforespørgsler direkte via kalenderen.

---

## 2. Hovedfunktionaliteter

### 📅 Godskalender (`UpcomingEvents.razor`)
En interaktiv månedskalender designet med en elegant herregårds-æstetik (creme-linen og guld):
*   **Klikbare datoceller**: Klik på en tom kalenderdato åbner øjeblikkeligt reservations-formularen med den valgte dato forudfyldt.
*   **Dobbelt-workflow Formular**: 
    *   *Koordinator-tilstand*: Tillader direkte oprettelse og lagring af bekræftede selskaber med tildelt koordinator.
    *   *Klient-tilstand*: Skjuler interne felter og lader eksterne gæster indsende en uforpligtende reservationsforespørgsel.
*   **Dagens Program (Subevents)**: Viser en kronologisk tidslinje over dagens programpunkter (f.eks. reception, middag, brudevals) med mulighed for inline oprettelse, redigering og sletning.

### 📋 Gods-Styringstavle (`KanbanBoard.razor`)
Et Kanban-board til strategisk styring af selskabernes afviklingsstadier:
*   **Træk-og-slip (Drag-and-Drop)**: Selskaber kan trækkes mellem kolonnerne (*Henvendelser, Sendte Tilbud, Bekræftede Bookinger, Planlægning, Afvikling, Afsluttet*) med øjeblikkelig MS Graph- og e-conomic synkronisering.
*   **Automatisering ved "Bekræftet"**: Når et selskab trækkes til kolonnen *"Bekræftede Bookinger"*, oprettes kunden automatisk i e-conomic, gods-pakker og forplejning beregnes, og en færdig bogført faktura med betalingslink genereres live.
*   **AI Gods-Assistent (Azure OpenAI GPT-4o)**: Genererer intelligente, selskabs-specifikke huskelister og klargøringsopgaver (f.eks. AV-opsætning i Hovedbygningen eller borddækning i Laden), som koordinatoren kan afkrydse interaktivt.
*   **Dagens Program Editor**: Fuld paritet med kalenderen – subevents kan tilføjes og redigeres direkte i Kanban-kortets redigeringsskuffe.

### ⚙️ Integrationscenter & Indstillinger (`Connect.razor`)
Det centrale kontrolpanel til gods-integrationer:
*   **Microsoft 365 Integration**: Autorisering via MSAL til læsning/skrivning af Outlook-fælleskalendere, afsendelse af bekræftelsesmails via Outlook samt lagring af Word/Excel dagsordener i OneDrive.
*   **Dynamics 365 Dataverse**: Service Principal konfiguration til synkronisering af kalender-ændringer direkte til Dataverse via OData-protokollen med indbygget automatisk entitets-pluralisering.
*   **PostgreSQL Backup (DigitalOcean)**: Backup-spejling af selskaber og del-begivenheder til en ekstern PostgreSQL-database på IP `104.248.47.45`. Indeholder en indbygget **SQL-DDL skemagenerator**.
*   **System Diagnostics Terminal**: En asynkron diagnosticeringskonsol, der pinger Key Vault, Graph API, Dataverse og PostgreSQL live og verificerer svartider og tabelforbindelser.
*   **Eksamens-Simulering**: En dedikeret guld-knap til eksamensbrug, der øjeblikkeligt simulerer 100% succesfuld forbindelse til alle cloud-tjenester og databaser uden behov for aktive logins.

---

## 3. Teknologisk Stak (Tech Stack)

Applikationen er bygget som en moderne, modulær Single-Page Application (SPA) med en fuldstændig decoblet enterprise-arkitektur:

### 💻 Frontend & Client
*   **Framework**: [C# / Blazor WebAssembly (WASM)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) under **.NET 8.0**. Applikationen kompileres direkte til WebAssembly og afvikles lynhurtigt i klientens browser.
*   **UI-Komponentbibliotek**: [MudBlazor](https://mudblazor.com/) (Material Design komponenter til Blazor) for et premium, responsivt og interaktivt layout.
*   **Designsystem**: Skræddersyet Vanilla CSS kombineret med MudBlazors tematisering for at opnå en luksuriøs herregårds-æstetik (creme `#FAF9F6`, dyb marineblå `#1E293B` og herregårdsguld `#D4AF37`).

### ☁️ Integrationer & Cloud API'er
*   **Microsoft Graph API (v5.40.0)**: Delegeret OAuth2 godkendelse til Outlook og OneDrive.
*   **Dynamics 365 Web API (OData v9.2)**: Integration til CRM- og Dataverse-miljøer.
*   **e-conomic REST API**: Bogføring, kunderegistrering og fakturagenerering.
*   **Azure OpenAI (GPT-4o API)**: Kognitive tjenester til intelligente huskelister og selskabsresuméer.
*   **PostgreSQL**: Ekstern backup-database hosted på en **DigitalOcean Droplet (`104.248.47.45:5432`)**.

### 🧪 Test & Kvalitetssikring
*   **Test-framework**: [xUnit](https://xunit.net/) til enhedstests og integrationstests.
*   **Mocking**: [Moq](https://github.com/moq/moq4) til asynkron emulering af HTTP-anmodninger og API-svar.
*   **Assertions**: [FluentAssertions](https://fluentassertions.com/) for læsbar testverifikation.
*   *Kodekvalitet*: Verificeret med **0 fejl og 0 advarsler** under striks .NET-kompilering og xUnit1031-analyzers.

### 🚀 CI/CD & Hosting
*   **Hosting**: Statisk hosting på **GitHub Pages** ([73n37.github.io](https://73n37.github.io/)).
*   **CI/CD Pipeline**: [GitHub Actions](https://github.com/features/actions), der automatisk bygger, tester og udruller applikationen ved hvert push til `main`-branchen. Indeholder case-insensitive brugernavns-detektering.
