using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Domain.Models;

namespace UnitTests.Domain
{
    public class ProductTests
    {
        [Theory]
        [InlineData("Produto 4", 15, 90, true)]
        [InlineData(null, 15, 90, false)]
        [InlineData("Produto 4", 0, 90, false)]
        [InlineData("Produto 4", 15, 0, false)]
        public void ValidateProductInfos(string productName, decimal price, int quantityAvaliable, bool result)
        {
            // Arrange
            var exception = false;

            try {
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    ProductName = productName,
                    Price = price,
                    QuantityAvaliable = quantityAvaliable
                };

                if (product.ProductName == null)
                {
                    throw new ArgumentException("Campos obrigatórios não podem ser nulos.");
                }
                if (product.Price <=0 || product.QuantityAvaliable <= 0) 
                {
                    throw new ArgumentException("Campo deve ser maior que 0.");
                }
            }
            catch (ArgumentException)
            {
                exception = true;
            }

            // Assert
            Assert.Equal(!result, exception);
        }
    }
}