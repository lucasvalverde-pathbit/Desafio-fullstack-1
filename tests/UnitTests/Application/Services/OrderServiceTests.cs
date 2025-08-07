using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;
using Moq;
using UnitTests.Mocks;
using Domain.Models;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Application.Services;
using Application.DTOs;
using Moq.Protected;

namespace UnitTests.Application.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            _orderService = new OrderService(
                _mockOrderRepository.Object,
                _mockProductRepository.Object,
                _mockCustomerRepository.Object,
                _httpClient
            );
        }

        [Theory]
        [InlineData(15, 0)]
        [InlineData(-10, 50)]
        public async Task CreateOrderAsync_InvalidCredentials(decimal price, int quantity)
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                UserType = UserType.Cliente,
                CustomerId = Guid.NewGuid(),
                Cep = "01001001",
                Products = new List<CreateOrderProductItem>
                {
                    new CreateOrderProductItem
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Produto 4",
                        Quantity = quantity,
                        Price = price
                    }
                }
            };

            // Act Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _orderService.CreateOrderAsync(request));
        }

        [Fact]
        public async Task CreateOrderAsync_ValidCredentials()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var request = new CreateOrderRequest
            {
                CustomerId = customerId,
                Cep = "35164318",
                UserType = UserType.Cliente,
                Products = new List<CreateOrderProductItem>
                {
                    new CreateOrderProductItem
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = "Produto 4",
                        Quantity = 2,
                        Price = 50
                    }
                }
            };

            var customer = new Customer
            {
                Id = customerId,
                CustomerName = "Cliente 1",
                CustomerEmail = "cliente1@gmail.com"
            };

            var product = new Product
            {
                Id = request.Products[0].ProductId,
                ProductName = "Produto 4",
                Price = 50,
                QuantityAvaliable = 80
            };

            _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId)).ReturnsAsync(customer);
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(request.Products[0].ProductId)).ReturnsAsync(product);
            _mockOrderRepository.Setup(repo => repo.CreateOrderAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

            _mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>
            ("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new[]
                {
                    new { addressName = "RUA VAIEVOLTA", districtName = "VENEZA", cityName = "IPATINGA", stateCode = "MG" }
                }))
            });

            // Act
            var result = await _orderService.CreateOrderAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.CustomerId, result.CustomerId);
            Assert.Equal(Status.Pendente, result.Status);
            Assert.Single(result.OrderProducts);
            Assert.Equal(100, result.Total);
        }

        [Fact]
        public async Task GetByCustomerIdAsyncValidCredentials()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var orders = new List<Order>
            {
                new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    Status = Status.Pendente,
                    Total = 200,
                    Cep = "35164318",
                    Customer = new Customer
                    {
                        Id = customerId,
                        CustomerName = "Cliente 1",
                        CustomerEmail = "cliente1@gmail.com"
                    }
                }
            };

            _mockOrderRepository.Setup(repo => repo.GetByCustomerIdAsync(customerId)).ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetByCustomerIdAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetCustomerById_InvalidCredentials()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            _mockOrderRepository.Setup(repo => repo.GetByCustomerIdAsync(customerId)).ReturnsAsync(new List<Order>());

            // Act
            var result = await _orderService.GetByCustomerIdAsync(customerId);

            // Assert 
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateOrderStatus_IfIdIsValid()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                Status = Status.Pendente,
                Cep = "35164318",
                Customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    CustomerName = "Cliente 1",
                    CustomerEmail = "cliente1@gmail.com"
                }
            };

            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId)).ReturnsAsync(order);
            _mockOrderRepository.Setup(repo => repo.UpdateOrderStatusAsync(orderId, It.IsAny<Status>())).Returns(Task.CompletedTask);

            // Act 
            await _orderService.UpdateOrderStatusAsync(orderId, Status.Enviado);

            // Assert 
            Assert.Equal(Status.Enviado, order.Status);
            _mockOrderRepository.Verify(repo => repo.UpdateOrderStatusAsync(orderId, Status.Enviado), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatus_IfIdIsInvalid()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId)).ReturnsAsync((Order)null);

            // Act Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _orderService.UpdateOrderStatusAsync(orderId, Status.Enviado));
        }

        [Fact]
        public async Task GetAddressFromCEPAsync_ValidCEP()
        {
            // Arrange
            var cep = "35164318";
            var address = "RUA VAIEVOLTA, VENEZA, IPATINGA - MG";

            _mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>
            ("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(new[]
                {
                    new { addressName = "RUA VAIEVOLTA", districtName = "VENEZA", cityName = "IPATINGA", stateCode = "MG" }
                }))
            });

            // Act 
            var result = await _orderService.GetAddressFromCEPAsync(cep);

            // Assert
            Assert.Equal(address, result);
        }

        [Fact]
        public async Task GetAddressFromCEPAsync_InvalidCEP()
        {
            // Arrange
            var cep = "99999999";

            _mockHttpMessageHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _orderService.GetAddressFromCEPAsync(cep));
            Assert.Contains("Erro ao buscar o endereço nas duas APIs", ex.Message);
        }
    }
}