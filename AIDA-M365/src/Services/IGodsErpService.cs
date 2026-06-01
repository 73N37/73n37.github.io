using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIDA.M365.Services;

/// <summary>
/// Provides unified ERP operations for Engestofte Gods bookings, communicating with the e-conomic REST API.
/// </summary>
public interface IGodsErpService
{
    /// <summary>
    /// Creates a new customer record in e-conomic, or retrieves the existing profile if the coordinator email matches.
    /// </summary>
    Task<EconomicCustomer> CreateOrGetCustomerAsync(string name, string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new draft invoice with itemized line items mapped to Gods packages, catering options, and staff services.
    /// </summary>
    Task<EconomicDraftInvoice> CreateDraftInvoiceAsync(int customerNumber, List<InvoiceLineItem> lines, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Books a draft invoice, producing a finalized PDF link and customer transaction payment URL.
    /// </summary>
    Task<EconomicBookedInvoice> BookInvoiceAsync(int draftInvoiceNumber, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a customer record profile in e-conomic.
/// </summary>
public class EconomicCustomer
{
    public int CustomerNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Represents an itemized line item inside an invoice.
/// </summary>
public class InvoiceLineItem
{
    public string ProductNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public decimal UnitNetPrice { get; set; }
}

/// <summary>
/// Represents a drafted, unbooked invoice in e-conomic.
/// </summary>
public class EconomicDraftInvoice
{
    public int DraftInvoiceNumber { get; set; }
    public int CustomerNumber { get; set; }
    public decimal NetAmount { get; set; }
    public decimal GrossAmount { get; set; }
}

/// <summary>
/// Represents a booked, finalized invoice in e-conomic.
/// </summary>
public class EconomicBookedInvoice
{
    public int BookedInvoiceNumber { get; set; }
    public string PdfUrl { get; set; } = string.Empty;
    public string PaymentLink { get; set; } = string.Empty;
}
