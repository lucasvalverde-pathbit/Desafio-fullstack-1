using Domain.Enums;

namespace Application.DTOs
{
    public class UpdateOrderStatusRequest
    {
        public Status Status { get; set; }
    }
}