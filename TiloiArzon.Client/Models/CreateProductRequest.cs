using Microsoft.AspNetCore.Components.Forms;

namespace TiloiArzon.Client.Models;

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public IBrowserFile? ImageFile { get; set; }
}

