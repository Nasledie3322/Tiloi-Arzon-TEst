namespace TiloiArzon.Client.Models
{
    public class OrderDtoClient
    {
        public int Id { get; set; }
        public List<OrderItemDtoClient> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class OrderItemDtoClient
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}