using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Domain.Models;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;

namespace UnitTests.Infrastructure.Repositories
{
    public class OrderRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly OrderRepository _orderRepository;

        public OrderRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "OrderTest")
                .Options;

            _dbContext = new AppDbContext(options);
            _orderRepository = new OrderRepository(_dbContext);
        }

        [Fact]
        public async Task CreateOrderAsync_ValidCredentials()
        {
            // Arrange 
            var customer = new Customer 
            {
                Id = Guid.NewGuid(),
                CustomerName = "Cliente 1",
                CustomerEmail = "cliente1@gmail.com"
            };

            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Produto 1",
                Price = 10.00m,
                QuantityAvaliable = 90
            };

            var order = new Order
            {
                CustomerId = customer.Id,
                Customer = customer,
                Status = Status.Pendente,
                Cep = "35164318",
                Address = "RUA VAIEVOLTA, VENEZA, IPATINGA - MG",
                Total = 30.00m,
                OrderProducts = new List<OrderProduct>
                {
                    new OrderProduct
                    {
                        OrderId = Guid.NewGuid(),
                        ProductId = product.Id,
                        ProductName = "Produto 1",
                        Product = product,
                        Price = product.Price,
                        Quantity = 3
                    }
                }
            };

            // Act 
            await _orderRepository.CreateOrderAsync(order);

            // Assert 
            var createdOrder = await _dbContext.Orders.Include(o => o.OrderProducts).FirstOrDefaultAsync(o => o.CustomerId == customer.Id);
            Assert.NotNull(createdOrder);
            Assert.Single(createdOrder.OrderProducts);
        }
        
        [Fact]
        public async Task GetAllAsync_IfOrderExists()
        {
            // Arrange
            var orders = await _orderRepository.GetAllAsync();

            // Assert 
            Assert.NotNull(orders);
        }

        [Fact]
        public async Task GetByCustomerIdAsync_IfIdExists()
        {
            // Arrange
            var customer = new Customer 
            {
                Id = Guid.NewGuid(),
                CustomerName = "Cliente 2",
                CustomerEmail = "cliente2@gmail.com"
            };
            
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Produto 2",
                Price = 50.00m,
                QuantityAvaliable = 80
            };

            var order = new Order
            {
                CustomerId = customer.Id,
                Customer = customer,
                Status = Status.Pendente,
                Cep = "35164318",
                Address = "RUA VAIEVOLTA, VENEZA, IPATINGA - MG",
                Total = 100.00m,
                OrderProducts = new List<OrderProduct>
                {
                    new OrderProduct
                    {
                        OrderId = Guid.NewGuid(),
                        ProductId = product.Id,
                        ProductName = "Produto 2",
                        Product = product,
                        Price = product.Price,
                        Quantity = 2
                    }
                }
            };

            _dbContext.Customers.Add(customer);
            _dbContext.Products.Add(product);
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetByCustomerIdAsync(customer.Id);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, o => Assert.Equal(customer.Id, o.CustomerId));
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_IfIsValid()
        {
            // Arrange
            var customer = new Customer 
            {
                Id = Guid.NewGuid(),
                CustomerName = "Cliente 3",
                CustomerEmail = "cliente3@gmail.com"
            };
            
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Produto 3",
                Price = 70.00m,
                QuantityAvaliable = 70
            };

            var order = new Order
            {
                CustomerId = customer.Id,
                Customer = customer,
                Status = Status.Pendente,
                Cep = "35164318",
                Address = "RUA VAIEVOLTA, VENEZA, IPATINGA - MG",
                Total = 70.00m,
                OrderProducts = new List<OrderProduct>
                {
                    new OrderProduct
                    {
                        OrderId = Guid.NewGuid(),
                        ProductId = product.Id,
                        ProductName = "Produto 3",
                        Product = product,
                        Price = product.Price,
                        Quantity = 1
                    }
                }
            };

            _dbContext.Customers.Add(customer);
            _dbContext.Products.Add(product);
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            await _orderRepository.UpdateOrderStatusAsync(order.Id, Status.Enviado);
            var updatedStatus = await _dbContext.Orders.FindAsync(order.Id);

            // Assert
            Assert.Equal(Status.Enviado, updatedStatus.Status);
        }

        [Fact]
        public async Task CreateOrderAsync_InvalidCredentials()
        {
            var customer  = new Customer
            {
                Id = Guid.NewGuid(),
                CustomerName = "Cliente 4",
                CustomerEmail = "customer4@gmail.com"
            };

            var order = new Order 
            {
                CustomerId = customer.Id,
                Customer = customer,
                Status = Status.Pendente,
                Cep = "00000000",
                Address = "Rua Inválida, 50",
                Total = -50.00m,
            };

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderRepository.CreateOrderAsync(order));
        }

        [Fact]
        public async Task GetByCustomerIdAsync_OrderNotFound()
        {
            // Act
            var result = await _orderRepository.GetByCustomerIdAsync(Guid.NewGuid());

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_OrderNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _orderRepository.UpdateOrderStatusAsync(Guid.NewGuid(), Status.Enviado));
        }
    }
}