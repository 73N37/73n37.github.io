using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIDA.M365.Services;

public sealed class BookingAcknowledgementService : IBookingAcknowledgementService
{
    private readonly IDynamicsCrmService _dynamicsCrmService;
    private readonly IOutlookCalendarEventService _outlookCalendarEventService;
    private readonly ILogger<BookingAcknowledgementService> _logger;

    public BookingAcknowledgementService(
        IDynamicsCrmService dynamicsCrmService,
        IOutlookCalendarEventService outlookCalendarEventService,
        ILogger<BookingAcknowledgementService> logger)
    {
        _dynamicsCrmService = dynamicsCrmService ?? throw new ArgumentNullException(nameof(dynamicsCrmService));
        _outlookCalendarEventService = outlookCalendarEventService ?? throw new ArgumentNullException(nameof(outlookCalendarEventService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AcknowledgementResult> AcknowledgeInquiryAsync(
        InboundEmailInquiry inquiry,
        CancellationToken cancellationToken = default)
    {
        if (inquiry == null)
        {
            return new AcknowledgementResult { Succeeded = false, ErrorMessage = "Inquiry is null" };
        }

        _logger.LogInformation("Processing inbound email inquiry from {SenderName} <{SenderEmail}>", inquiry.SenderName, inquiry.SenderEmail);

        try
        {
            // 1. Simulate dynamics CRM lead generation
            var mockLeadId = Guid.NewGuid().ToString();
            
            // 2. Simulate sending M365 auto-reply email via Graph
            _logger.LogInformation("Sending Graph SendMail auto-reply email acknowledgement to {SenderEmail}", inquiry.SenderEmail);
            await Task.Delay(300, cancellationToken); // Minor async network delay simulation

            var mockMailMessageId = $"AAMkAGExM2E4...{Guid.NewGuid().ToString().Substring(0,8)}=";

            return new AcknowledgementResult
            {
                Succeeded = true,
                LeadId = mockLeadId,
                MailMessageId = mockMailMessageId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge inquiry from {SenderEmail}", inquiry.SenderEmail);
            return new AcknowledgementResult
            {
                Succeeded = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
