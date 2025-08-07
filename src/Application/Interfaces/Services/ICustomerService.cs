// Interface que define a assinatura de métodos específicos de CustomerService
using Domain.Models;

namespace Application.Interfaces.Services
{
    public interface ICustomerService : IGenericService<Customer> {}
}