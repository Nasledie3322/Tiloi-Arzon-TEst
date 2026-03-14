using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace TiloiArzon.Client.Services;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ITokenStore _tokenStore;

    public JwtAuthStateProvider(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) return Anonymous;
        if (JwtClaims.IsExpired(token, DateTimeOffset.UtcNow)) return Anonymous;

        var claims = JwtClaims.ParseClaims(token).ToList();

        // Normalize role claim so AuthorizeView/Authorize(Roles=...) works
        var roleClaims = claims.Where(c => c.Type is "role" or "roles").ToList();
        foreach (var role in roleClaims)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Value));
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyAuthStateChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}

