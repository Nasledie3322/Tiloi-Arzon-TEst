using TiloiArzon.Client.Models;

namespace TiloiArzon.Client.Services;

public class OrdersApi : ApiServiceBase
{
    public OrdersApi(HttpClient http, ITokenStore tokenStore) : base(http, tokenStore) { }

    public async Task<List<OrderDto>> GetMyOrdersAsync()
    {
        await EnsureAuthHeaderAsync();
        try
        {
            return await Http.GetFromJsonAsync<List<OrderDto>>("api/orders") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<OrderDto?> CheckoutAsync()
    {
        await EnsureAuthHeaderAsync();
        var resp = await Http.PostAsync("api/orders/checkout", content: null);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<OrderDto>();
    }
}
