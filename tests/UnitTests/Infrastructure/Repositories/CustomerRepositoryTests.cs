using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories;

namespace UnitTests.Infrastructure.Repositories
{
    public class CustomerRepositoryTests
    {
        private readonly AppDbContext _dbContext; 
        private readonly CustomerRepository _customerRepository;

        public CustomerRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "CustomerTest")
                .Options;

            _dbContext = new AppDbContext(options);
            _customerRepository = new CustomerRepository(_dbContext);
        }

        [Fact]
        public async Task ExistsAsync_IfCustomerExists()
        {
            // Arrange 
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                CustomerName = "Cliente 1",
                CustomerEmail = "cliente1@gmail.com"
            };

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();
            
            // Act
            var result = await _customerRepository.ExistsAsync(customer.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ErrorIfCustomerNotFound()
        {
            // Act 
            var result = await _customerRepository.ExistsAsync(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }
    }
}