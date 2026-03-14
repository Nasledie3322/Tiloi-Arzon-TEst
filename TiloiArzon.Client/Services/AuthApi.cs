using TiloiArzon.Client.Models;

namespace TiloiArzon.Client.Services;

public class AuthApi : ApiServiceBase
{
    private readonly JwtAuthStateProvider _authStateProvider;
    private readonly ITokenStore _tokenStore;

    public AuthApi(HttpClient http, ITokenStore tokenStore, JwtAuthStateProvider authStateProvider)
        : base(http, tokenStore)
    {
        _tokenStore = tokenStore;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        var response = await Http.PostAsJsonAsync("api/auth/register", dto);
        if (!response.IsSuccessStatusCode) return null;
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (payload?.Token is { Length: > 0 })
        {
            await _tokenStore.SetTokenAsync(payload.Token);
            _authStateProvider.NotifyAuthStateChanged();
        }
        return payload;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await Http.PostAsJsonAsync("api/auth/login", dto);
        if (!response.IsSuccessStatusCode) return null;
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (payload?.Token is { Length: > 0 })
        {
            await _tokenStore.SetTokenAsync(payload.Token);
            _authStateProvider.NotifyAuthStateChanged();
        }
        return payload;
    }

    public async Task LogoutAsync()
    {
        await _tokenStore.ClearAsync();
        _authStateProvider.NotifyAuthStateChanged();
    }
}

