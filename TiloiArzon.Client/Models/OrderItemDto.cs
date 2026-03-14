namespace TiloiArzon.Client.Models;

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? ProductName { get; set; }
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }
}

