using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIDA.M365.Services;

public interface IEconomicErpService
{
    Task<EconomicCustomer> CreateOrGetCustomerAsync(string name, string email, CancellationToken cancellationToken = default);
    Task<EconomicDraftInvoice> CreateDraftInvoiceAsync(int customerNumber, List<InvoiceLineItem> lines, CancellationToken cancellationToken = default);
    Task<EconomicBookedInvoice> BookInvoiceAsync(int draftInvoiceNumber, CancellationToken cancellationToken = default);
}

public class EconomicCustomer
{
    public int CustomerNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class InvoiceLineItem
{
    public string ProductNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public decimal UnitNetPrice { get; set; }
}

public class EconomicDraftInvoice
{
    public int DraftInvoiceNumber { get; set; }
    public int CustomerNumber { get; set; }
    public decimal NetAmount { get; set; }
    public decimal GrossAmount { get; set; }
}

public class EconomicBookedInvoice
{
    public int BookedInvoiceNumber { get; set; }
    public string PdfUrl { get; set; } = string.Empty;
    public string PaymentLink { get; set; } = string.Empty;
}
