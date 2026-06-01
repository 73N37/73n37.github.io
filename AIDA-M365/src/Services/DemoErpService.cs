using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIDA.M365.Services;

/// <summary>
/// Mock invoice creation service simulating the e-conomic ERP API integration for offline sandbox testing.
/// Generates authentic-feeling random invoice identifiers, tax rates, and customer profiles.
/// </summary>
public sealed class DemoErpService : IGodsErpService
{
    private static readonly Random _random = new();

    /// <inheritdoc />
    public async Task<EconomicCustomer> CreateOrGetCustomerAsync(
        string name,
        string email,
        CancellationToken cancellationToken = default)
    {
        // Simulate minor API delay for real tactile responsiveness
        await Task.Delay(400, cancellationToken);

        int mockCustomerNumber = _random.Next(10000, 99999);
        return new EconomicCustomer
        {
            CustomerNumber = mockCustomerNumber,
            Name = name,
            Email = email
        };
    }

    /// <inheritdoc />
    public async Task<EconomicDraftInvoice> CreateDraftInvoiceAsync(
        int customerNumber,
        List<InvoiceLineItem> lines,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(300, cancellationToken);

        int mockDraftNumber = _random.Next(5000, 9999);
        decimal netSum = 0;
        foreach (var item in lines)
        {
            netSum += item.UnitNetPrice * (decimal)item.Quantity;
        }

        return new EconomicDraftInvoice
        {
            DraftInvoiceNumber = mockDraftNumber,
            CustomerNumber = customerNumber,
            NetAmount = netSum,
            GrossAmount = netSum * 1.25m // Standard 25% Danish VAT
        };
    }

    /// <inheritdoc />
    public async Task<EconomicBookedInvoice> BookInvoiceAsync(
        int draftInvoiceNumber,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(500, cancellationToken);

        int mockBookedNumber = _random.Next(100000, 999999);
        string pdfUrl = $"https://restapi-sandbox.e-conomic.com/invoices/booked/{mockBookedNumber}/pdf";
        string paymentLink = $"https://payment.e-conomic.com/invoice/{mockBookedNumber}/pay?token=sandbox_pay_tok_eleanor_kensington";

        return new EconomicBookedInvoice
        {
            BookedInvoiceNumber = mockBookedNumber,
            PdfUrl = pdfUrl,
            PaymentLink = paymentLink
        };
    }
}
