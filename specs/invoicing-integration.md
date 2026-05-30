# Specification: e-conomic ERP Invoicing Integration

This document defines the strict integration guidelines with the **e-conomic ERP REST API** (V1.0.0).

## 1. REST Endpoints & Authentication
- **Staging/Dev Endpoint**: `https://restapi-sandbox.e-conomic.com`
- **Production Endpoint**: `https://restapi.e-conomic.com`
- **Headers Required**:
  - `X-AppSecretToken`: Dedicated developer application token.
  - `X-AgreementGrantToken`: Active client subscription authorization token.
  - `Content-Type`: `application/json`

## 2. Customer Synchronisation
When a reservation is moved to the **Confirmed** stage:
1. Lookup customer in e-conomic by email: `GET /customers?filter=email$eq:{email}`
2. If found, retrieve `customerNumber`.
3. If not found, create new customer: `POST /customers`
   ```json
   {
     "name": "Client Full Name",
     "currency": "DKK",
     "customerGroup": { "customerGroupNumber": 1 },
     "vatZone": { "vatZoneNumber": 1 },
     "email": "client@email.com",
     "paymentTerms": { "paymentTermsNumber": 1 }
   }
   ```

## 3. Invoice Execution Pipeline
Once the customer is resolved, the system provisions an invoice:
1. **Create Draft Invoice**: `POST /invoices/drafts`
   ```json
   {
     "date": "2026-05-30",
     "currency": "DKK",
     "customer": { "customerNumber": 1001 },
     "recipient": {
       "name": "Client Full Name",
       "vatZone": { "vatZoneNumber": 1 }
     },
     "lines": [
       {
         "lineNumber": 1,
         "description": "Grand Ballroom Estate Rental Fee",
         "quantity": 1.0,
         "unitNetPrice": 12500.00,
         "product": { "productNumber": "PROD-001" }
       }
     ]
   }
   ```
2. **Book Invoice**: To lock the transaction and generate invoice numbers/PDFs: `POST /invoices/booked`
   ```json
   {
     "draftInvoice": {
       "draftInvoiceNumber": 4059
     }
   }
   ```
3. **Retrieve PDF & Payment Link**:
   - The PDF URL is retrieved from the booked invoice endpoint: `GET /invoices/booked/{invoiceNumber}/pdf`
   - Retrieve transaction URLs and include them in the dispatch email alongside the formal booking contract PDF.
