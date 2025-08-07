// Interface que define a assinatura de métodos específicos de ProductService
using Domain.Models;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IProductService : IGenericService<Product>
    {
        Task<ProductDTO> CreateProductAsync(ProductDTO productDto);
    }

}