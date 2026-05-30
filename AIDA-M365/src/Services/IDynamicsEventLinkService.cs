using AIDA.M365.Models;

namespace AIDA.M365.Services;

public interface IDynamicsEventLinkService
{
    Task UpdateLinkedEventAsync(
        DynamicsEventUpdate update,
        CancellationToken cancellationToken = default);
}
