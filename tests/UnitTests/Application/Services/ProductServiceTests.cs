using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Domain.Models;
using Domain.Interfaces.Repositories;
using Application.DTOs;
using Application.Services;

namespace UnitTests.Application.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _productService = new ProductService(_mockProductRepository.Object);
        }

        [Theory]
        [InlineData("Produto 4", 20.0, 30, true)]
        [InlineData(null, 20.0, 30, false)]
        [InlineData("Produto 4", -20.0, 30, false)]
        [InlineData("Produto 4", 20.0, 0, false)]
        public async Task CreateProductAsync(string productName, decimal price, int quantityAvaliable, bool success)
        {
            // Arrange
            var productDto = new ProductDTO
            {
                ProductName = productName,
                Price = price,
                QuantityAvaliable = quantityAvaliable
            };

            if (success)
            {
                _mockProductRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
                    .Returns(Task.CompletedTask);
            }

            // Act Assert
            if (success)
            {
                var result = await _productService.CreateProductAsync(productDto);

                Assert.NotNull(result);
                Assert.Equal(productDto.ProductName, result.ProductName);
                Assert.Equal(productDto.Price, result.Price);
                Assert.Equal(productDto.QuantityAvaliable, result.QuantityAvaliable);

                _mockProductRepository.Verify( repo => repo.AddAsync(It.IsAny<Product>()), Times.Once);
            }
            else 
            {
                await Assert.ThrowsAsync<ArgumentException>(() => _productService.CreateProductAsync(productDto));
                _mockProductRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Never);
            }
        }
    }
}