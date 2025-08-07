namespace Application.DTOs
{
    public class ProductDTO
    {
        public Guid Id { get; set; }
        public required string ProductName { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvaliable { get; set; }
    }
}
