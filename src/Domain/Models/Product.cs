using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string ProductName { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvaliable { get; set; }
        public List<OrderProduct> OrderProducts { get; set; } = new();
    }
}