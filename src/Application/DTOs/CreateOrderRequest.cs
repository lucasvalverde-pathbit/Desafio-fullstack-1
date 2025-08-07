using Domain.Models;
using Domain.Enums;

namespace Application.DTOs
{
    public class CreateOrderRequest
    {
        public Guid CustomerId { get; set; }
        public List<CreateOrderProductItem> Products { get; set; } = new();
        public required string Cep { get; set; }
        public UserType UserType { get; set; }
    }
}
