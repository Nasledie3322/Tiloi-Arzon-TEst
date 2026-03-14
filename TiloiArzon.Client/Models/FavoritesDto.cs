namespace TiloiArzon.Client.Models;
public class FavoriteDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}