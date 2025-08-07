using System.Transactions;
using System.Net.Http;
using Newtonsoft.Json;
using Domain.Models;
using Domain.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.DTOs;
using Domain.Enums;

namespace Application.Services
{
    public class OrderService : GenericService<Order>, IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly HttpClient _httpClient;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, ICustomerRepository customerRepository, HttpClient httpClient) : base(orderRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _httpClient = httpClient;
        }

        // Criar um pedido
        public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                throw new Exception("Cliente não encontrado.");
            }

            if (request.UserType != UserType.Cliente)
            {
                throw new UnauthorizedAccessException("Apenas clientes podem realizar pedidos.");
            }

            var address = await GetAddressFromCEPAsync(request.Cep);
            if (string.IsNullOrEmpty(address))
            {
                throw new InvalidOperationException("Endereço não encontrado, CEP inválido.");
            }

            var order = new Order
            {
                CustomerId = request.CustomerId,
                Customer = customer,
                OrderDate = DateTime.UtcNow,
                Status = Status.Pendente,
                Total = 0,
                Cep = request.Cep,
                Address = address,
                OrderProducts = new List<OrderProduct>()
            };

            decimal totalValue = 0;

            var orderProducts = new List<OrderProduct>();

            foreach(var requestProduct in request.Products)
            {
                var product = await _productRepository.GetByIdAsync(requestProduct.ProductId);

                if (product == null)
                {
                    throw new Exception($"Produto com ID {requestProduct.ProductId} não encontrado.");
                }
                if (product.QuantityAvaliable < requestProduct.Quantity)
                {
                    throw new Exception($"Quantidade insuficiente no estoque para o produto {requestProduct.ProductId}.");
                }

                var orderProduct = new OrderProduct
                {
                    Order = order,
                    OrderId = order.Id,
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    Quantity = requestProduct.Quantity,
                    Product = product,
                    Price = product.Price
                };

                orderProducts.Add(orderProduct);

                totalValue += product.Price * requestProduct.Quantity;

                product.QuantityAvaliable -= requestProduct.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            order.Total = totalValue;
            order.Status = Status.Pendente;
            order.OrderProducts = orderProducts;
            await _orderRepository.CreateOrderAsync(order);
        
            return order;
        }

        // Buscar pedidos por ID de cliente
        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid id)
        {
            return await _orderRepository.GetByCustomerIdAsync(id);
        }

        // Mudar o status do pedido
        public async Task UpdateOrderStatusAsync(Guid orderId, Status newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order is null)
            {
                throw new KeyNotFoundException("Pedido não encontrado.");
            }
            newStatus = Status.Enviado;

            order.Status = newStatus;
            await _orderRepository.UpdateOrderStatusAsync(orderId, newStatus);
        }

        // Função para buscar o endereço pelo CEP
        public async Task<string> GetAddressFromCEPAsync(string cep)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://ceprapido.com.br/api/addresses/{cep}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var addressData = JsonConvert.DeserializeObject<List<dynamic>>(content);
                var firstAddress = addressData[0];

                var formattedAddress = $"{firstAddress.addressName}, {firstAddress.districtName}, {firstAddress.cityName} - {firstAddress.stateCode}";
                return formattedAddress;
            }
            catch (Exception)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"https://opencep.com/v1/{cep}");
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var address = JsonConvert.DeserializeObject<dynamic>(content);

                    var formattedAddress = $"{address.logradouro}, {address.bairro}, {address.localidade} - {address.uf}";
                    return formattedAddress;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao buscar o endereço nas duas APIs. Detalhes: {ex.Message}");
                }
            }
        }
    }
}