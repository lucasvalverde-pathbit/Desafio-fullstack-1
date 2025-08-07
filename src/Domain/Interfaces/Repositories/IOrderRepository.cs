// Assinatura de métodos específicos que serão implementados em OrderRepository
using Domain.Models;
using Domain.Enums;

namespace Domain.Interfaces.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
        Task CreateOrderAsync(Order order);
        Task UpdateOrderStatusAsync(Guid orderId, Status newStatus);
    }
}