using System.Threading;
using System.Threading.Tasks;
using AIDA.M365.Models;

namespace AIDA.M365.Services;

public interface IDynamicsCrmService
{
    Task<DynamicsCrmProfile?> GetProfileAsync(
        string entityLogicalName,
        string entityId,
        CancellationToken cancellationToken = default);
}
