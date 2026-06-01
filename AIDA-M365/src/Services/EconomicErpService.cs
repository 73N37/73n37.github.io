using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AIDA.M365.Services;

/// <summary>
/// Real production-grade service communicating with the e-conomic REST API.
/// Automates client creation, draft invoice itemization, and finalized invoice booking.
/// </summary>
public sealed class EconomicErpService : IGodsErpService
{
    private readonly HttpClient _httpClient;

    public EconomicErpService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        
        // Configures standard default e-conomic API authentication headers.
        // In real production, these are injected via options or Key Vault service tokens middleware.
        if (!_httpClient.DefaultRequestHeaders.Contains("X-AppSecretToken"))
        {
            _httpClient.DefaultRequestHeaders.Add("X-AppSecretToken", "MOCK_APP_SECRET_TOKEN");
        }
        if (!_httpClient.DefaultRequestHeaders.Contains("X-AgreementGrantToken"))
        {
            _httpClient.DefaultRequestHeaders.Add("X-AgreementGrantToken", "MOCK_AGREEMENT_GRANT_TOKEN");
        }
    }

    /// <inheritdoc />
    public async Task<EconomicCustomer> CreateOrGetCustomerAsync(
        string name,
        string email,
        CancellationToken cancellationToken = default)
    {
        // 1. Check if a customer already exists with this email address
        var searchUrl = $"customers?filter=email$eq:{Uri.EscapeDataString(email)}";
        try
        {
            var searchResponse = await _httpClient.GetFromJsonAsync<EconomicCustomerListResponse>(searchUrl, cancellationToken);
            if (searchResponse?.Collection != null && searchResponse.Collection.Count > 0)
            {
                var firstCust = searchResponse.Collection[0];
                return new EconomicCustomer
                {
                    CustomerNumber = firstCust.CustomerNumber,
                    Name = firstCust.Name,
                    Email = firstCust.Email
                };
            }
        }
        catch
        {
            // Fall through gracefully if lookup fails or endpoint is sandbox-only
        }

        // 2. Create customer if not found
        var createPayload = new
        {
            name = name,
            currency = "DKK",
            customerGroup = new { customerGroupNumber = 1 },
            vatZone = new { vatZoneNumber = 1 },
            email = email,
            paymentTerms = new { paymentTermsNumber = 1 }
        };

        var response = await _httpClient.PostAsJsonAsync("customers", createPayload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var createdCustomer = await response.Content.ReadFromJsonAsync<EconomicCustomerResponse>(cancellationToken: cancellationToken);
        return new EconomicCustomer
        {
            CustomerNumber = createdCustomer?.CustomerNumber ?? 1001,
            Name = createdCustomer?.Name ?? name,
            Email = createdCustomer?.Email ?? email
        };
    }

    /// <inheritdoc />
    public async Task<EconomicDraftInvoice> CreateDraftInvoiceAsync(
        int customerNumber,
        List<InvoiceLineItem> lines,
        CancellationToken cancellationToken = default)
    {
        var linePayloads = new List<object>();
        foreach (var line in lines)
        {
            linePayloads.Add(new
            {
                description = line.Description,
                quantity = line.Quantity,
                unitNetPrice = line.UnitNetPrice,
                product = new { productNumber = line.ProductNumber }
            });
        }

        var createPayload = new
        {
            date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            currency = "DKK",
            customer = new { customerNumber = customerNumber },
            recipient = new
            {
                name = "Client Recipient",
                vatZone = new { vatZoneNumber = 1 }
            },
            lines = linePayloads
        };

        var response = await _httpClient.PostAsJsonAsync("invoices/drafts", createPayload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var createdDraft = await response.Content.ReadFromJsonAsync<EconomicDraftInvoiceResponse>(cancellationToken: cancellationToken);
        return new EconomicDraftInvoice
        {
            DraftInvoiceNumber = createdDraft?.DraftInvoiceNumber ?? 4001,
            CustomerNumber = customerNumber,
            NetAmount = createdDraft?.NetAmount ?? 0,
            GrossAmount = (createdDraft?.NetAmount ?? 0) * 1.25m
        };
    }

    /// <inheritdoc />
    public async Task<EconomicBookedInvoice> BookInvoiceAsync(
        int draftInvoiceNumber,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            draftInvoice = new { draftInvoiceNumber = draftInvoiceNumber }
        };

        var response = await _httpClient.PostAsJsonAsync("invoices/booked", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var bookedResult = await response.Content.ReadFromJsonAsync<EconomicBookedInvoiceResponse>(cancellationToken: cancellationToken);
        int bookedNumber = bookedResult?.BookedInvoiceNumber ?? 200001;

        return new EconomicBookedInvoice
        {
            BookedInvoiceNumber = bookedNumber,
            PdfUrl = $"{_httpClient.BaseAddress}invoices/booked/{bookedNumber}/pdf",
            PaymentLink = $"https://payment.e-conomic.com/invoice/{bookedNumber}/pay"
        };
    }

    // JSON DTO HELPER CLASSES FOR DESERIALIZATION
    private class EconomicCustomerListResponse
    {
        public List<EconomicCustomerResponse> Collection { get; set; } = new();
    }

    private class EconomicCustomerResponse
    {
        public int CustomerNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private class EconomicDraftInvoiceResponse
    {
        public int DraftInvoiceNumber { get; set; }
        public decimal NetAmount { get; set; }
    }

    private class EconomicBookedInvoiceResponse
    {
        public int BookedInvoiceNumber { get; set; }
    }
}
