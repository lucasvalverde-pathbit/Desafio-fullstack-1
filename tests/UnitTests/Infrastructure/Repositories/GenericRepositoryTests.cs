using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;

namespace UnitTests.Infrastructure.Repositories
{
    public class GenericRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly GenericRepository<Product> _genericRepository;

        public GenericRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "GenericTest")
                .Options;

            _dbContext = new AppDbContext(options);
            _genericRepository = new GenericRepository<Product>(_dbContext);
        }

        [Fact]
        public async Task GetAllAsync()
        {
            _dbContext.Products.RemoveRange(_dbContext.Products);
            await _dbContext.SaveChangesAsync();

            // Arrange 
            _dbContext.Products.AddRange(new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Produto 4",
                    Price = 15,
                    QuantityAvaliable = 50
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Produto 5",
                    Price = 16,
                    QuantityAvaliable = 50
                },
            });

            await _dbContext.SaveChangesAsync();

            // Act 
            var result = await _genericRepository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_IfIdExists()
        {
            _dbContext.Products.RemoveRange(_dbContext.Products);
            await _dbContext.SaveChangesAsync();

            // Arrage
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Produto 4",
                Price = 15,
                QuantityAvaliable = 50
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _genericRepository.GetByIdAsync(product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal("Produto 4", result.ProductName);
        }

        [Fact]
        public async Task GetByIdAsync_IfIdNotFound()
        {
            // Arrange 
            var productId = Guid.NewGuid();
            
            // Act Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _genericRepository.GetByIdAsync(productId));
        }

        [Fact]
        public async Task AddAsync_ValidCredentials()
        {
            _dbContext.Products.RemoveRange(_dbContext.Products);
            await _dbContext.SaveChangesAsync();

            // Arrange
            var product = new Product
            {
                ProductName = "Produto 4",
                Price = 15,
                QuantityAvaliable = 50
            };

            // Act
            await _genericRepository.AddAsync(product);
            var savedProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductName == "Produto 4");

            // Assert
            Assert.NotNull(savedProduct);
            Assert.Equal(15, savedProduct.Price);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            _dbContext.Products.RemoveRange(_dbContext.Products);
            await _dbContext.SaveChangesAsync();
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Produto 4",
                Price = 20,
                QuantityAvaliable = 70
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            product.Price = 25;
            await _genericRepository.UpdateAsync(product);

            var updatedProduct = await _dbContext.Products.FindAsync(product.Id);

            // Assert
            Assert.NotNull(updatedProduct);
            Assert.Equal(25, updatedProduct.Price);
        }

        [Fact]
        public async Task DeleteAsync()
        {
            _dbContext.Products.RemoveRange(_dbContext.Products);
            await _dbContext.SaveChangesAsync();
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductName = "Produto 4",
                Price = 20,
                QuantityAvaliable = 50
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            // Act
            await _genericRepository.DeleteAsync(productId);

            var deletedProduct = await _dbContext.Products.FindAsync(product.Id);

            // Assert
            Assert.Null(deletedProduct);
        }
    }
}