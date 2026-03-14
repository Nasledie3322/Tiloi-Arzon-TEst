using TiloiArzon.Client.Models;

namespace TiloiArzon.Client.Services;

public class CategoriesApi : ApiServiceBase
{
    public CategoriesApi(HttpClient http, ITokenStore tokenStore) : base(http, tokenStore) { }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await Http.GetFromJsonAsync<List<CategoryDto>>("api/categories") ?? new();
    }
}

