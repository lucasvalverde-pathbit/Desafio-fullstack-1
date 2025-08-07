using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Domain.Models;

namespace UnitTests.Domain
{
    public class OrderProductTests
    {
        [Fact]
        public void AddProductToOrder()
        {
            var product1 = new Product
            {
                ProductName = "Produto 4",
                Price = 13.99m,
                QuantityAvaliable = 30
            };

            var product2 = new Product
            {
                ProductName = "Produto 5",
                Price = 15.00m,
                QuantityAvaliable = 40
            };

            var customerId = Guid.NewGuid();
            var order = new Order
            {
                Cep = "35164318",
                Customer = new Customer
                    {
                        Id = customerId,
                        CustomerName = "Cliente 1",
                        CustomerEmail = "cliente1@gmail.com"
                    },
                OrderProducts = new List<OrderProduct>(),
            };

            var orderProduct1 = new OrderProduct
            {
                OrderId = order.Id,
                Order = order, 
                ProductId = product1.Id,
                ProductName = product1.ProductName,
                Product = product1,
                Quantity = 3,
                Price = product1.Price
            };

            var orderProduct2 = new OrderProduct
            {
                OrderId = order.Id,
                Order = order,
                ProductId = product2.Id,
                ProductName = product2.ProductName,
                Product = product2,
                Quantity = 2,
                Price = product2.Price
            };

            // Act
            order.OrderProducts.Add(orderProduct1);
            order.OrderProducts.Add(orderProduct2);

            // Assert
            Assert.Contains(orderProduct1, order.OrderProducts);
            Assert.Contains(orderProduct2, order.OrderProducts);
            Assert.Equal(2, order.OrderProducts.Count);
        }
    }
}