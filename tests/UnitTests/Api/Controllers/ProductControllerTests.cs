using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Api.Controllers;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests.Api.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly ProductController _productController;
        public ProductControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _productController = new ProductController(_mockProductService.Object);
        }

        [Fact]
        public async Task Create_IfProductIsValid()
        {
            // Arrange
            var productDto = new ProductDTO
            {
                ProductName = "Product 1",
                Price = 50.00m,
                QuantityAvaliable = 50
            };

            var createdProduct = new ProductDTO
            {
                Id = Guid.NewGuid(),
                ProductName = "Product 1",
                Price = 50.00m,
                QuantityAvaliable = 50
            };

            _mockProductService.Setup(service => service.CreateProductAsync(It.IsAny<ProductDTO>()))
                .ReturnsAsync(createdProduct);

            // Act
            var result = await _productController.Create(productDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<ProductDTO>(createdResult.Value);
            Assert.Equal(createdProduct.Id, returnValue.Id);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenQuantityIsInvalid()
        {
            // Arrange
            var productDto = new ProductDTO
            {
                ProductName = "Produto 2",
                Price = 50.0m,
                QuantityAvaliable = 0
            };

            // Act
            var result = await _productController.Create(productDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Dados inválidos. A quantidade deve ser maior que zero.", badRequestResult.Value);
        }
    }
}