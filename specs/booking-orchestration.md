# Specification: Booking Orchestration & Inquiry Flow

## 1. Booking Widget Availability
- Prospective clients view availability on a public, embedded **Microsoft Bookings page**.
- Bookings are mapped to specific estate venue resources (Søparken, HovedBygnigen, Den Store Lade).
- The public widget represents occupied slots using standard "busy" blocks, enforcing a **minimum 14-day lead time** and **maximum 365-day lead time**.
- Buffer zones of **2 hours pre-event** (for catering setup) and **2 hours post-event** (for cleaning crews) are automatically injected as blockouts in Graph Outlook.

## 2. Inbound Email Flow
```
[Customer sends inquiry email] 
             |
             v
[M365 Shared Mailbox (reservations@royalestate.com)]
             |
             v (Subject match: "Wedding", "Birthday", "Reservation")
[Power Automate Cloud Flow Trigger]
             |
             +---> Parse body via Azure OpenAI GPT-4o
             +---> Write Lead/Contact into Dynamics 365 CRM
             +---> Save event payload to Azure SQL cache
             +---> Send auto-acknowledgement (HTML) email to client
```

## 3. Auto-Acknowledgement Template
The acknowledgement email is standard responsive HTML incorporating cream/gold design assets:
- **Sender**: Shared mailbox (`reservations@engelstoftegods.com`)
- **Attachments**: Estate Brochure PDF and Terms sheet
- **Subject**: "Royal Estate Reservation Request Received - [Client Name]"
- **Body**: Custom summary indicating that their request has been received, and an event coordinator will review the booking slot manually.
