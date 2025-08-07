using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Api.Controllers;
using Application.DTOs;
using Application.Interfaces.Services;
using Common.DTOs;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UnitTests.Api.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly UserController _userController;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _userController = new UserController(_mockUserService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Signup_ValidCredentials()
        {
            // Arrange
            var signUpRequest = new SignUpRequest
            {
                Name = "User 1",
                UserName = "user1",
                UserEmail = "user1@gmail.com",
                Password = "user1senha",
                UserType = UserType.Administrador.ToString()
            };

            _mockUserService.Setup(service => service.SignupAsync(
                signUpRequest.Name,
                signUpRequest.UserName,
                signUpRequest.UserEmail,
                signUpRequest.Password,
                Enum.Parse<UserType>(signUpRequest.UserType)))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userController.Signup(signUpRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseMessageDTO;
            Assert.Equal("Usuário cadastrado com sucesso.", response.Message);
        }

        [Fact]
        public async Task UpdateUser_IfUserExists()
        {
            // Arrange
            var updateUserDTO = new UpdateUserDTO
            {
                Name = "User 3",
                UserName = "user 3",
                UserEmail = "user3@gmail.com",
                Password = "user3senha"
            };

            var userId = Guid.NewGuid();
            _mockUserService.Setup(service => service.UpdateUserAsync(userId, updateUserDTO)).ReturnsAsync(true);

            // Act
            var result = await _userController.UpdateUser(userId, updateUserDTO);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as ResponseMessageDTO;
            Assert.Equal("Perfil atualizado com sucesso", response.Message);
        }

        [Fact]
        public async Task UpdateUser_IfUserNotFound()
        {
            // Arrange
            var updateUserDTO = new UpdateUserDTO
            {
                Name = "User 4",
                UserName = "user4",
                UserEmail = "user4@gmail.com",
                Password = "user4senha"
            };

            var userId = Guid.NewGuid();
            _mockUserService.Setup(service => service.UpdateUserAsync(userId, updateUserDTO))
                .ReturnsAsync(false);

            // Act
            var result = await _userController.UpdateUser(userId, updateUserDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value as ResponseMessageDTO;
            Assert.Equal("Erro ao atualizar perfil", response.Message);
        }
    }
}