namespace Application.DTOs
{
    public class CreateOrderProductItem {
        public Guid ProductId { get; set; }
        public required string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}