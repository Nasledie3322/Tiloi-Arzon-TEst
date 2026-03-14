using System.Net.Http.Headers;

namespace TiloiArzon.Client.Services;

public abstract class ApiServiceBase
{
    protected readonly HttpClient Http;
    private readonly ITokenStore _tokenStore;

    protected ApiServiceBase(HttpClient http, ITokenStore tokenStore)
    {
        Http = http;
        _tokenStore = tokenStore;
    }

    protected async Task EnsureAuthHeaderAsync()
    {
        var token = await _tokenStore.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            Http.DefaultRequestHeaders.Authorization = null;
            return;
        }

        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}

