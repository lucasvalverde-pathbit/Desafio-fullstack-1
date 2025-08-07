namespace Domain.Models
{
    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string CustomerName { get; set; }
        public required string CustomerEmail { get; set; }
    }
}