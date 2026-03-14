using TiloiArzon.Client.Models;

namespace TiloiArzon.Client.Services;

public class AdminProductsApi : ApiServiceBase
{
    public AdminProductsApi(HttpClient http, ITokenStore tokenStore) : base(http, tokenStore) { }

    public async Task<ProductDto?> CreateAsync(CreateProductRequest request)
    {
        await EnsureAuthHeaderAsync();

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.Name), "Name");
        content.Add(new StringContent(request.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price");
        content.Add(new StringContent(request.StockQuantity.ToString(System.Globalization.CultureInfo.InvariantCulture)), "StockQuantity");
        content.Add(new StringContent(request.CategoryId.ToString(System.Globalization.CultureInfo.InvariantCulture)), "CategoryId");
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            content.Add(new StringContent(request.Description), "Description");
        }

        if (request.ImageFile != null)
        {
            var fileContent = new StreamContent(request.ImageFile.OpenReadStream(request.ImageFile.Size));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ImageFile.ContentType);
            content.Add(fileContent, "imageFile", request.ImageFile.Name);
        }

        var resp = await Http.PostAsync("api/admin/products", content);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<ProductDto>();
    }

    public async Task<ProductDto?> UpdateAsync(int id, CreateProductRequest request)
    {
        await EnsureAuthHeaderAsync();

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(request.Name), "Name");
        content.Add(new StringContent(request.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price");
        content.Add(new StringContent(request.StockQuantity.ToString(System.Globalization.CultureInfo.InvariantCulture)), "StockQuantity");
        content.Add(new StringContent(request.CategoryId.ToString(System.Globalization.CultureInfo.InvariantCulture)), "CategoryId");
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            content.Add(new StringContent(request.Description), "Description");
        }

        if (request.ImageFile != null)
        {
            var fileContent = new StreamContent(request.ImageFile.OpenReadStream(request.ImageFile.Size));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ImageFile.ContentType);
            content.Add(fileContent, "imageFile", request.ImageFile.Name);
        }

        var resp = await Http.PutAsync($"api/admin/products/{id}", content);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<ProductDto>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await EnsureAuthHeaderAsync();
        var resp = await Http.DeleteAsync($"api/admin/products/{id}");
        return resp.IsSuccessStatusCode;
    }
}
