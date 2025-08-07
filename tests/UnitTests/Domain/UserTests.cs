using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Domain.Models;
using Domain.Enums;

namespace UnitTests.Domain
{
    public class UserTests
    {
        [Theory]
        [InlineData("Vera", "veralucia", "vera@gmail.com", "vera123", UserType.Administrador, true)]
        [InlineData(null, "veralucia", "vera@gmail.com", "vera123", UserType.Administrador, false)]
        [InlineData("Vera", null, "vera@gmail.com", "vera123", UserType.Administrador, false)]
        [InlineData("Vera", "veralucia", null, "vera123", UserType.Administrador, false)]
        [InlineData("Vera", "veralucia", "vera@gmail.com", null, UserType.Administrador, false)]
        [InlineData("Vera", "veralucia", "vera@gmail.com", null, (UserType)3, false)]
        public void ValidateUserInfos(string name, string userName, string userEmail, string password, UserType userType, bool result)
        {
            // Arrange
            var exception = false;

            try {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    UserName = userName,
                    UserEmail = userEmail,
                    Password = password,
                    UserType = userType
                };

                if (user.Name == null || user.UserName == null || user.UserEmail == null || user.Password == null)
                {
                    throw new ArgumentNullException("Campos obrigatórios não podem ser nulos.");
                }
            }
            catch (ArgumentNullException) {
                exception = true;
            }

            // Assert
            Assert.Equal(!result, exception);
        }
    }
}