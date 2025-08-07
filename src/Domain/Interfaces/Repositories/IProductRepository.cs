// Assinatura de métodos específicos que serão implementados em ProductRepository
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IProductRepository : IGenericRepository<Product> {}
}