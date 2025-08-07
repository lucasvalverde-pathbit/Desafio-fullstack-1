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
    public class CustomerControllerTests
    {
        private readonly Mock<ICustomerService> _mockCustomerService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly CustomerController _customerController;

        public CustomerControllerTests()
        {
            _mockCustomerService = new Mock<ICustomerService>();
            _mockUserService = new Mock<IUserService>();
            _customerController = new CustomerController(_mockCustomerService.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task Create_ReturnBadRequest()
        {
            // Act 
            var result = _customerController.Create();

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = Assert.IsType<ResponseMessageDTO>(badRequest.Value);
            Assert.Equal("Clientes são criados automaticamente ao cadastrar um usuário do tipo 'Cliente'.", value.Message);
        }

        [Fact]
        public async Task UpdateUser_IfIdExists()
        {
            // Arrange 
            var userId = Guid.NewGuid();
            var updateUserDTO = new UpdateUserDTO
            {
                Name = "Cliente 1",
                UserName = "cliente1",
                UserEmail = "cliente1@gmail.com",
                Password = "cliente1senha"
            };

            _mockUserService.Setup(s => s.UpdateUserAsync(userId, updateUserDTO)).ReturnsAsync(true);

            // Act
            var result = await _customerController.UpdateUser(userId, updateUserDTO);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Perfil atualizado com sucesso", actionResult.Value);
        }

        [Fact]
        public async Task UpdateUser_ErrorIfIdNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateUserDTO = new UpdateUserDTO
            {
                Name = "Cliente 2",
                UserName = "cliente2",
                UserEmail = "cliente2@gmail.com",
                Password = "cliente2senha"
            };

            _mockUserService.Setup(s => s.UpdateUserAsync(userId, updateUserDTO)).ReturnsAsync(false);

            // Act
            var result = await _customerController.UpdateUser(userId, updateUserDTO);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Erro ao atualizar perfil", actionResult.Value);
        }
    }
}