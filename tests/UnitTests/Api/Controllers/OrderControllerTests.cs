using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Xunit;
using Moq;
using Domain.Models;
using Domain.Enums;
using Api.Controllers;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UnitTests.Api.Controllers
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly OrderController _orderController;

        public OrderControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockHttpClient = new Mock<HttpClient>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _orderController = new OrderController(_mockOrderService.Object, _mockHttpClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Create_ErrorIfRequestIsNull()
        {
            // Act
            var result = await _orderController.Create(null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseMessageDTO>(badRequest.Value);
            Assert.Equal("Dados inválidos.", response.Message);
        }

        [Fact]
        public async Task Create_ErrorIfProductQuantityNotFound()
        {
            // Arrange
            var createOrderRequest = new CreateOrderRequest
            {
                CustomerId = Guid.NewGuid(),
                Cep = "35164318",
                UserType = UserType.Cliente,
                Products = new List<CreateOrderProductItem>()
            };

            // Act
            var result = await _orderController.Create(createOrderRequest);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseMessageDTO>(badRequest.Value);
            Assert.Equal("É obrigatório adicionar produtos ao pedido.", response.Message);
        }

        [Fact]
        public async Task Create_ErrorIfQuantityOrPriceIsZeroOrNegative()
        {
            // Arrange
            var createOrderRequest = new CreateOrderRequest
            {
                CustomerId = Guid.NewGuid(),
                Cep = "35164318",
                UserType = UserType.Cliente,
                Products = new List<CreateOrderProductItem>
                {
                    new CreateOrderProductItem
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Produto 1",
                        Quantity = 0,
                        Price = 50
                    },
                    new CreateOrderProductItem 
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Produto 2",
                        Quantity = 10,
                        Price = -5
                    }
                }
            };

            // Act
            var result = await _orderController.Create(createOrderRequest);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseMessageDTO>(badRequest.Value);
            Assert.Equal("É obrigatório preencher as quantidades e o valor unitário de todos os produtos", response.Message);
        }

        [Fact]
        public async Task Create_ValidCredentials()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            var customer = new Customer
            {
                Id = customerId,
                CustomerName = "Cliente 2",
                CustomerEmail = "cliente2@gmail.com"
            };

            var createOrderRequest = new CreateOrderRequest
            {
                CustomerId = customerId,
                Cep = "35164318",
                UserType = UserType.Cliente,
                Products = new List<CreateOrderProductItem>
                {
                    new CreateOrderProductItem
                    { 
                        ProductId = Guid.NewGuid(), 
                        ProductName = "Produto 3", 
                        Quantity = 1, 
                        Price = 50 
                    }
                }
            };

            var createdOrder = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Customer = customer,
                Cep = "35164318",
                Status = Status.Pendente
            };

            _mockOrderService.Setup(service => service.CreateOrderAsync(createOrderRequest)).ReturnsAsync(createdOrder);

            // Act
            var result = await _orderController.Create(createOrderRequest);

            // Assert
            var createdAtAction= Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetById", createdAtAction.ActionName);
            Assert.Equal(createdOrder.Id, createdAtAction.RouteValues["id"]);
        }

        [Fact]
        public async Task UpdateStatus_ErrorIfInvalidStatus()
        {
            // Arrange
            var updateOrderStatusRequest = new UpdateOrderStatusRequest { Status = (Status)3 };
            var orderId = Guid.NewGuid();

            // Act
            var result = await _orderController.UpdateStatus(orderId, updateOrderStatusRequest);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseMessageDTO>(badRequest.Value);
            Assert.Equal("Status inválido.", response.Message);
        }

        [Fact]
        public async Task UpdateStatus_ValidCredentials()
        {
            // Arrange
            var updateOrderStatusRequest = new UpdateOrderStatusRequest
            { 
                Status = Status.Enviado 
            };

            var orderId = Guid.NewGuid();

            _mockOrderService.Setup(service => service.UpdateOrderStatusAsync(orderId, updateOrderStatusRequest.Status))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _orderController.UpdateStatus(orderId, updateOrderStatusRequest);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}