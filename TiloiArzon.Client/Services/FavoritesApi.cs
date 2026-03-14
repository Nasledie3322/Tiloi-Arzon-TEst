using TiloiArzon.Client.Models;

namespace TiloiArzon.Client.Services;

public class FavoritesApi : ApiServiceBase
{
    public FavoritesApi(HttpClient http, ITokenStore tokenStore) : base(http, tokenStore) { }

    public async Task<List<FavoriteDto>> GetAllAsync()
    {
        await EnsureAuthHeaderAsync();
        try
        {
            return await Http.GetFromJsonAsync<List<FavoriteDto>>("api/favorites") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<bool> AddAsync(int productId)
    {
        await EnsureAuthHeaderAsync();
        var resp = await Http.PostAsync($"api/favorites/{productId}", content: null);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveAsync(int favoriteId)
    {
        await EnsureAuthHeaderAsync();
        var resp = await Http.DeleteAsync($"api/favorites/{favoriteId}");
        return resp.IsSuccessStatusCode;
    }
}
