using System.Collections.Generic;


namespace TiloiArzon.Client.Models

{
    public class CreateOrderDto
    {
        public required List<OrderItemDto> Items { get; set; }
    }
}