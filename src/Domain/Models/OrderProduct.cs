using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class OrderProduct
    {   
        public Guid OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
        public Guid ProductId { get; set; }
        [Column(TypeName = "text")]
        public required string ProductName { get; set; }
        [JsonIgnore]
        public Product Product { get; set ;}
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}