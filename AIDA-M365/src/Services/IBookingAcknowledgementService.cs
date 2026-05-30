using System.Threading;
using System.Threading.Tasks;

namespace AIDA.M365.Services;

public interface IBookingAcknowledgementService
{
    Task<AcknowledgementResult> AcknowledgeInquiryAsync(InboundEmailInquiry inquiry, CancellationToken cancellationToken = default);
}

public class InboundEmailInquiry
{
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public class AcknowledgementResult
{
    public bool Succeeded { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string LeadId { get; set; } = string.Empty;
    public string MailMessageId { get; set; } = string.Empty;
}
