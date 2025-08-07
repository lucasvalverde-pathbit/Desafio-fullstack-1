using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Domain.Models;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;

namespace UnitTests.Infrastructure.Repositories
{
    public class UserRepositoryTests
    {
        private readonly AppDbContext  _dbContext;
        private readonly UserRepository _userRepository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "UserTest")
                .Options;

            _dbContext = new AppDbContext(options);
            _userRepository = new UserRepository(_dbContext);
        }

        [Fact]
        public async Task GetByIdAsync_IfIdExists()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Administrador 1",
                UserName = "admin1",
                UserEmail = "admin1@gmail.com",
                Password = "administrador1",
                UserType = UserType.Administrador
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetByIdAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal("admin1", result.UserName);
        }

        [Fact]
        public async Task GetByIdAsync_IfIdNotFound()
        {
            // Act
            var result = await _userRepository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_IfUserExists()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Administrador 2",
                UserName = "admin2",
                UserEmail = "admin2@gmail.com",
                Password = "administrador2",
                UserType = UserType.Administrador
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            user.UserName = "administrador 2";
            await _userRepository.UpdateAsync(user);
            var updatedUser = await _userRepository.GetByIdAsync(user.Id);

            // Assert
            Assert.NotNull(updatedUser);
            Assert.Equal("administrador 2", updatedUser.UserName);
        }

        [Fact]
        public async Task UpdateAsync_ErrorIfUserNotFound()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "administrador 3",
                UserName = "admin3",
                UserEmail = "admin3@gmail.com",
                Password = "administrador3",
                UserType = UserType.Administrador
            };

            // Act Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _userRepository.UpdateAsync(user));
        }

        [Fact]
        public async Task GetByEmailAsync_IfEmailExists()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Administrador 4",
                UserName = "admin4",
                UserEmail = "admin4@gmail.com",
                Password = "administrador4",
                UserType = UserType.Administrador
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetByEmailAsync("admin4@gmail.com");

            // Assert   
            Assert.NotNull(result);
            Assert.Equal("admin4@gmail.com", result.UserEmail);
        }

        [Fact]
        public async Task GetByEmailAsync_IfEmailNotFound()
        {
            var result = await _userRepository.GetByEmailAsync("administrador@gmail.com");
            Assert.Null(result);
        }

        [Fact]
        public async Task SignupAsync_ValidCredentials()
        {
            // Act
            var user = await _userRepository.SignupAsync
            (
                "Administrador 5",
                "admin5",
                "admin5@gmail.com",
                "administrador5",
                UserType.Administrador
            );

            // Assert
            Assert.NotNull(user);
            Assert.Equal("admin5", user.UserName);
            Assert.NotEqual("administrador5", user.Password);
        }

        [Fact]
        public async Task SignupAsync_InvalidCredentials()
        {
            // Arrange
            await _userRepository.SignupAsync
            (
                "Administrador 6",
                "admin6",
                "admin@gmail.com",
                "administrador6",
                UserType.Administrador
            );

            // Act Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userRepository.SignupAsync
                (
                    "Administrador 7",
                    "admin7",
                    "admin@gmail.com",
                    "administrador7",
                    UserType.Administrador
                )
            );

            Assert.Equal("E-mail já cadastrado.", exception.Message);
        }
    }
}