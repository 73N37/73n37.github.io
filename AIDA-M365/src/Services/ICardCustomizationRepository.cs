using AIDA.M365.Models;

namespace AIDA.M365.Services;

public interface ICardCustomizationRepository
{
    Task SaveAsync(
        CardCustomization customization,
        CancellationToken cancellationToken = default);
}
