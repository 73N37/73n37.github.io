using System.Net.Http.Json;
using AIDA.M365.Models;

namespace AIDA.M365.Services;

public sealed class CardCustomizationApiRepository : ICardCustomizationRepository
{
    private readonly HttpClient _httpClient;

    public CardCustomizationApiRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SaveAsync(
        CardCustomization customization,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/card-customizations",
            customization,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
