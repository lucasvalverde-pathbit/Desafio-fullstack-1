// Assinatura de métodos específicos que serão implementados em CustomerRepository
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<bool> ExistsAsync(Guid id);
    }
}
