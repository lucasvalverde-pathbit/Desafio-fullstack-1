using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Domain.Models;
using Domain.Interfaces.Repositories;
using Application.Services;
using Application.Interfaces.Services;

namespace UnitTests.Application.Services
{
    public class GenericServiceTests
    {
        private readonly Mock<IGenericRepository<Product>> _mockGenericRepository;
        private readonly IGenericService<Product> _genericService;

        public GenericServiceTests()
        {
            _mockGenericRepository = new Mock<IGenericRepository<Product>>();
            _genericService = new GenericService<Product>(_mockGenericRepository.Object);
        }

        [Fact]
        public async Task GetByIdAsync()
        {
            // Arrange 
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductName = "Produto 4",
                Price = 15.00m,
                QuantityAvaliable = 50
            };

            _mockGenericRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);

            // Act
            var result = await _genericService.GetByIdAsync(productId);

            // Assert 
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Produto 4", result.ProductName);
            Assert.Equal(15, result.Price);
            Assert.Equal(50, result.QuantityAvaliable);
        }

        [Fact]
        public async Task GetAllAsync()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Produto 4",
                    Price = 15.00m,
                    QuantityAvaliable = 50
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Produto 5",
                    Price = 16.00m,
                    QuantityAvaliable = 60
                }
            };

            _mockGenericRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _genericService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task CreateAsync()
        {
            // Arrange
            var product = new Product 
            {
                Id = Guid.NewGuid(),
                ProductName = "Produto 4",
                Price = 15.00m,
                QuantityAvaliable = 50
            };

            // Act
            await _genericService.CreateAsync(product);

            // Assert
            _mockGenericRepository.Verify(repo => repo.AddAsync(product), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // Arrange
            var product = new Product 
            { 
                Id = Guid.NewGuid(),
                ProductName = "Produto 5",
                Price = 35.00m,
                QuantityAvaliable = 30
            };

            // Act
            await _genericService.UpdateAsync(product);

            // Assert
            _mockGenericRepository.Verify(repo => repo.UpdateAsync(product), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync()
        {
            // Arrange 
            var productId = Guid.NewGuid();

            // Act
            _genericService.DeleteAsync(productId);

            // Assert
            _mockGenericRepository.Verify(repo => repo.DeleteAsync(productId), Times.Once);
        }
    }
}