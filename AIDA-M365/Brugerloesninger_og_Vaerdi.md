# Brugerløsninger & Værdi: AIDA-M365

Dette dokument beskriver, hvordan **AIDA-M365 (Strategic Event Command Center)** løser kritiske, daglige problemer for slutbrugeren (projektledere, salgsteam, eventkoordinatorer og ledere), som arbejder i Microsoft 365- og Dynamics 365-økosystemerne.

Platformen bygger bro mellem kalenderstyring, CRM-data og intelligent processtyring i én samlet, visuel brugerflade.

---

## 🛑 De 5 Største Frustrationer (Før AIDA-M365)

Før AIDA-M365 stod medarbejdere over for en række ineffektive processer, når de forsøgte at styre strategiske begivenheder og kundemøder:

### 1. Datafragmentering og "App-Swapping"
Medarbejdere var tvunget til konstant at skifte mellem adskilte applikationer: **Outlook** for at se kalendere, **Dynamics 365** for at slå kundeoplysninger op, og eksterne værktøjer som **Trello** for at få et Kanban-baseret procesoverblik. Dette konstante skift dræner produktiviteten og skaber mentalt træthed.

### 2. Manuel og Fejlagtig Dobbelt-Synkronisering
Hvis en mødetid eller en deadline ændrede sig på et uafhængigt Kanban-board (f.eks. Trello), skulle medarbejderen manuelt gå ind i Outlook og opdatere kalenderbegivenheden, og efterfølgende ind i Dynamics 365 for at opdatere salgsmuligheden. Det førte ofte til glemte opdateringer, dobbeltbookinger og forvirring i teamet.

### 3. Kalendermøder Uden Forretningskontekst
En standard Outlook-kalender viser *hvem* der deltager, og *hvornår* mødet finder sted, men den mangler den dybere forretningsmæssige kontekst. Det er svært at se, hvilken specifik salgsmulighed (Opportunity) eller kunde (Account) et møde er koblet til, uden at skulle lede i CRM-systemet.

### 4. Informationsmætning og Lange Mødebeskrivelser
Mødeindkaldelser er ofte fyldt med ustruktureret information, lange mailkorrespondancer eller komplekse dagsordener. Det tager tid for medarbejderen at læse det hele igennem for at finde ud af, hvad de faktiske handlingspunkter (action items) og beslutninger er.

### 5. Manglende Processtyring i Kalenderen
En traditionel kalenderliste eller ugevisning er fantastisk til tidsstyring, men elendig til processtyring. Den viser ikke, hvilket stadie en begivenhed eller et salgsmøde befinder sig i (f.eks. "Planlagt", "Forberedt", "Opfølgning påkrævet" eller "Afsluttet").

---

## ⚡ Hvordan AIDA-M365 Løser Disse Problemer (Efter AIDA-M365)

AIDA-M365 transformerer måden, teams arbejder på, ved at introducere en række intelligente, automatiserede løsninger:

### 🌟 1. Ét Samlet Visuelt Kontrolcenter (Kanban)
I stedet for at kigge på en tætpakket kalender eller en tør CRM-liste, får brugeren et elegant Kanban-board (drevet af MudBlazor). Kolonnerne repræsenterer enten procesfaser, tidsintervaller eller Dynamics-eventkategorier. 
* **Værdi:** Brugeren får øjeblikkeligt et 360-graders overblik over alle begivenheder og deres nuværende status.

### 🔄 2. Tovejs-Synkronisering via Drag-and-Drop
Når en bruger trækker et kort fra én kolonne til en anden på Kanban-boardet, sker der tre ting helt automatisk i baggrunden:
1. **Microsoft Graph API** opdaterer øjeblikkeligt start- og sluttidspunktet for det rigtige møde i Outlook.
2. **Dynamics 365 Web API** opdaterer status eller tilknytning på den tilkoblede salgsmulighed eller kunde.
3. **Azure SQL** gemmer brugerens visuelle præferencer (f.eks. kortets farve).
* **Værdi:** Ingen manuel dobbeltindtastning. Kalenderen, CRM-systemet og Kanban-boardet er altid 100% synkroniseret.

### 🔗 3. Direkte Forbindelse mellem Kalender og CRM
Hvert kort på Kanban-boardet viser ikke blot mødeoplysningerne, men trækker også live-data direkte fra Dynamics 365. Brugeren kan med et enkelt blik se, hvilken **Kunde (Account)** eller **Salgsmulighed (Opportunity)** mødet er knyttet til.
* **Værdi:** Møder får øjeblikkelig forretningsmæssig kontekst, hvilket sikrer bedre forberedelse og målrettet kundekontakt.

### 🤖 4. AI-drevet "Smart Summarize" (Tidsbesparelse)
Med et enkelt klik på et kort aktiveres **Azure OpenAI (`gpt-4o-mini`)**. AI-modellen analyserer automatisk hele mødebeskrivelsen og eventuelle vedhæftede tekster.
* **Værdi:** Brugeren præsenteres på få sekunder for et ultrakort resumé og en klar liste over konkrete **handlingspunkter (action items)**. Medarbejderen sparer værdifuld tid på forberedelse og opfølgning.

### 🛡️ 5. Stressfri Fejlhåndtering (Pålidelighed)
Hvis en synkronisering fejler (f.eks. hvis der ikke er internetforbindelse, eller hvis Dynamics 365 er nede), viser systemet et tydeligt indlæsnings-overlay og ruller automatisk kortet tilbage til dets oprindelige plads og tidspunkt.
* **Værdi:** Brugeren kan stole 100% på systemet. Der sker aldrig delvise opdateringer, hvor kalenderen ændres, men CRM-systemet ikke gør.

---

## 📈 Kvantificerbar Værdi for Virksomheden

Ved at implementere AIDA-M365 opnår virksomheden markante fordele:
* **Tidsbesparelse:** Op til 30-45 minutter sparet om dagen pr. medarbejder ved eliminering af app-swapping, manuel synkronisering og manuel læsning af lange mødetekster.
* **Øget Datakvalitet:** CRM-systemet (Dynamics 365) holdes altid opdateret, fordi opdateringen sker naturligt som en del af medarbejderens daglige visuelle workflow.
* **Hurtigere Onboarding:** Nye medarbejdere kan let overskue komplekse salgsprocesser og møderækker via det intuitive Kanban-layout.
* **Bedre Kundeservice:** Kunder oplever et mere velforberedt team, der altid har styr på aftaler, historik og handlingspunkter.
