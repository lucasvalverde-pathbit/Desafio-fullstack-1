using Application.DTOs;
using Domain.Models;
using Domain.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class ProductService : GenericService<Product>, IProductService
    {
        private readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository) : base (productRepository)
        {
            _productRepository = productRepository;
        }

        // Criar um produto
        public async Task<ProductDTO> CreateProductAsync(ProductDTO productDto)
        {
            if (string.IsNullOrEmpty(productDto.ProductName))
            {
                throw new ArgumentException("O nome do produto não pode ser nulo.");
            }

            if (productDto.Price <= 0)
            {
                throw new ArgumentException("O preço do produto deve ser maior que zero.");
            }

            if(productDto.QuantityAvaliable <= 0)
            {
                throw new ArgumentException("A quantidade do produto deve ser maior que zero.");
            }

            var product = new Product()
            {
                ProductName = productDto.ProductName,
                Price = productDto.Price,
                QuantityAvaliable = productDto.QuantityAvaliable
            };
            
            await _productRepository.AddAsync(product);

            var productDtoResponse = new ProductDTO
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Price = product.Price,
                QuantityAvaliable = product.QuantityAvaliable
            };

            return productDtoResponse;
        }
    }
}