using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace AIDA.M365.Services;

/// <summary>
/// Bridges the Blazor WASM MSAL token provider with the Microsoft Graph SDK v5.
/// Intercepts every outgoing Graph request and attaches the current user's
/// OAuth2 Bearer token obtained via MSAL silently (or prompts login if expired).
/// </summary>
public sealed class MsalGraphAuthenticationProvider : IAuthenticationProvider
{
    private readonly Microsoft.AspNetCore.Components.WebAssembly.Authentication.IAccessTokenProvider _tokenProvider;

    // The Graph permission scopes this app needs
    private static readonly string[] GraphScopes =
    [
        "https://graph.microsoft.com/User.Read",
        "https://graph.microsoft.com/Calendars.ReadWrite"
    ];

    public MsalGraphAuthenticationProvider(
        Microsoft.AspNetCore.Components.WebAssembly.Authentication.IAccessTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    /// <summary>
    /// Called automatically by the Graph SDK before every HTTP request.
    /// Attaches the MSAL-obtained Bearer token to the Authorization header.
    /// </summary>
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        var tokenResult = await _tokenProvider.RequestAccessToken(
            new AccessTokenRequestOptions { Scopes = GraphScopes });

        if (tokenResult.TryGetToken(out var token))
        {
            request.Headers.Add("Authorization", $"Bearer {token.Value}");
        }
    }
}
