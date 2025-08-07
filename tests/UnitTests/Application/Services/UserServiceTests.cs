using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Xunit;
using Moq;
using Domain.Models;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using Application.DTOs;
using Application.Services;
using Application.Interfaces.Services;

namespace UnitTests.Application.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ICustomerService> _mockCustomerService;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockCustomerService = new Mock<ICustomerService>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("minha-chave-secreta-muito-segura-123");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("MeuSistema");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("MeuSistemaParaUsuarios");

            _userService = new UserService
            (
                _mockUserRepository.Object,
                _mockCustomerService.Object,
                _mockLogger.Object,
                _mockConfiguration.Object
            );
        }

        [Theory]
        [InlineData("User1", "user1", "user1@gmail.com", "user1senha", UserType.Administrador, true, false)]
        [InlineData("User2", "user2", "user2@gmail.com", "user2senha", UserType.Cliente, true, true)]
        [InlineData(null, "user3", "user3@gmail.com", "user3senha", UserType.Administrador, false, false)]
        [InlineData("User4", null, "user4@gmail.com", "user4senha", UserType.Cliente, false, true)]
        [InlineData("User5", "user5", null, "user5senha", UserType.Administrador, false, false)]
        [InlineData("User6", "user6", "user6@gmail.com", null, UserType.Cliente, false, true)]
        [InlineData("User7", "user7", "user7@gmail.com", "user7senha", (UserType)3, false, false)]
        public async Task SignupAsync(
            string name,
            string userName,
            string userEmail,
            string password,
            UserType userType, 
            bool success,
            bool createCustomer
        )
        {
            // Arrange
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(userEmail)).ReturnsAsync((User)null);
            _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            if (success)
            {
                await _userService.SignupAsync(name, userName, userEmail, password, userType);

                // Assert
                _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);

                if (createCustomer)
                {
                    _mockCustomerService.Verify(service => service.CreateAsync(It.IsAny<Customer>()), Times.Once);
                }
                else 
                {
                    _mockCustomerService.Verify(service => service.CreateAsync(It.IsAny<Customer>()), Times.Never);
                }
            }
            else
            {
                await Assert.ThrowsAsync<ArgumentException>(() =>
                    _userService.SignupAsync(name, userName, userEmail, password, userType)
                );
                _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
                _mockCustomerService.Verify(service => service.CreateAsync(It.IsAny<Customer>()), Times.Never);
            }
        }

        [Fact]
        public async Task SignupAsync_ThrowsExceptionIfEmailExists()
        {
            // Arrange
            var user = new User
            {
                Name = "User Existente",
                UserName = "user1",
                UserEmail = "user1emailexistente@gmail.com",
                Password = "user1senha",
                UserType = UserType.Cliente
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync("user1emailexistente@gmail.com")).ReturnsAsync(user);

            // Act Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _userService.SignupAsync("User Existente", "user1", "user1emailexistente@gmail.com", "user1senha", UserType.Cliente)
            );

            _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "User1",
                UserName = "user1",
                UserEmail = "user1@gmail.com",
                Password = HashPassword("user1senha"),
                UserType = UserType.Administrador
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync("user1@gmail.com")).ReturnsAsync(user);

            // Act
            var token = await _userService.LoginAsync("user1@gmail.com", "user1senha");

            // Assert 
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials()
        {
            // Arrange
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync("useremailinvalido@gmail.com")).ReturnsAsync((User)null);

            // Act Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _userService.LoginAsync("useremailinvalido@gmail.com", "user1")
            );
        }

        [Fact]
        public async Task UpdateUserAsync_IfUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Name = "User1",
                UserName = "user1",
                UserEmail = "user1@gmail.com",
                Password = HashPassword("user1senha")
            };

            var updateUserDto = new UpdateUserDTO
            {
                Name = "User",
                UserName = "user",
                UserEmail = "user@gmail.com",
                Password = "usersenhasegura"
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserRepository.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateUserDto);

            // Assert
            Assert.True(result);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
        }
        
        [Fact]
        public async Task UpdateUserAsync_FalseIfUserNotNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateUserDto = new UpdateUserDTO
            {
                Name = "User1",
                UserName = "user1",
                UserEmail = "user1@gmail.com",
                Password = HashPassword("user1senha")
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateUserDto);

            // Assert
            Assert.False(result);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        // Apenas para hash de senha
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}