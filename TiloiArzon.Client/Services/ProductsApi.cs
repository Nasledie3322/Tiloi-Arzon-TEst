using TiloiArzon.Client.Models;

namespace TiloiArzon.Client.Services;

public class ProductsApi : ApiServiceBase
{
    public ProductsApi(HttpClient http, ITokenStore tokenStore) : base(http, tokenStore) { }

    public async Task<List<ProductDto>> GetAllAsync(string? searchTerm = null, int? categoryId = null)
    {
        var url = "api/products";
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(searchTerm)) query.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
        if (categoryId.HasValue) query.Add($"categoryId={categoryId.Value}");
        if (query.Count > 0) url += "?" + string.Join("&", query);

        return await Http.GetFromJsonAsync<List<ProductDto>>(url) ?? new();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        return await Http.GetFromJsonAsync<ProductDto>($"api/products/{id}");
    }
}

