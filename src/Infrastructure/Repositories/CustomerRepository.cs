using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Domain.Interfaces.Repositories;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        // Construtor para receber o AppDbContext e passar pra base GenericRepository
        public CustomerRepository(AppDbContext context) : base(context) {}

        // Método implementado para encontrar todos os clientes
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id);
        }
    }
}
