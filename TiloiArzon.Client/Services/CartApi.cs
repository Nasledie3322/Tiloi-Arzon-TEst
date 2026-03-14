using TiloiArzon.Client.Models;

namespace TiloiArzon.Client.Services;

public class CartApi : ApiServiceBase
{
    public CartApi(HttpClient http, ITokenStore tokenStore) : base(http, tokenStore) { }

    public async Task<List<CartItemDto>> GetAsync()
    {
        await EnsureAuthHeaderAsync();
        try
        {
            return await Http.GetFromJsonAsync<List<CartItemDto>>("api/cart") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<List<CartItemDto>> AddAsync(int productId, int quantity = 1)
    {
        await EnsureAuthHeaderAsync();
        var resp = await Http.PostAsJsonAsync("api/cart/items", new { ProductId = productId, Quantity = quantity });
        if (!resp.IsSuccessStatusCode) return new();
        return await resp.Content.ReadFromJsonAsync<List<CartItemDto>>() ?? new();
    }

    public async Task<List<CartItemDto>> SetQuantityAsync(int productId, int quantity)
    {
        await EnsureAuthHeaderAsync();
        var resp = await Http.PutAsJsonAsync($"api/cart/items/{productId}", new { Quantity = quantity });
        if (!resp.IsSuccessStatusCode) return new();
        return await resp.Content.ReadFromJsonAsync<List<CartItemDto>>() ?? new();
    }

    public async Task<List<CartItemDto>> RemoveAsync(int productId)
    {
        await EnsureAuthHeaderAsync();
        var resp = await Http.DeleteAsync($"api/cart/items/{productId}");
        if (!resp.IsSuccessStatusCode) return new();
        return await resp.Content.ReadFromJsonAsync<List<CartItemDto>>() ?? new();
    }
}
