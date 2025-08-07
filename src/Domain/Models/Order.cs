using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Domain.Enums;

namespace Domain.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime OrderDate { get; set; } = DateTime.Now;
        [Column(TypeName = "text")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Status Status { get; set; }
        public decimal Total { get; set; }
        public required string Cep { get; set; }
        public string Address { get; set; }
        public Guid CustomerId { get; set; }
        [JsonIgnore]
        public Customer Customer { get; set; }
        [JsonInclude]
        public List<OrderProduct> OrderProducts { get; set; } = new();
    }
}
