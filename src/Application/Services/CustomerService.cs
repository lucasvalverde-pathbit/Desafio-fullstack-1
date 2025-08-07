using Domain.Models;
using Domain.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class CustomerService : GenericService<Customer>, ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        public CustomerService(ICustomerRepository customerRepository) : base (customerRepository)
        {
            _customerRepository = customerRepository;
        }
    }
}