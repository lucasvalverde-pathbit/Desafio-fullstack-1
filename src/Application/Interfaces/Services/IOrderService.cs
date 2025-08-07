// Interface que define a assinatura de métodos específicos de OrderService
using Application.DTOs;
using Domain.Models;
using Domain.Enums;

namespace Application.Interfaces.Services
{
    public interface IOrderService : IGenericService<Order>
    {
        Task<Order> CreateOrderAsync(CreateOrderRequest request);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid id);
        Task UpdateOrderStatusAsync(Guid orderId, Status newStatus);
        Task<string> GetAddressFromCEPAsync(string cep);
    }
}
