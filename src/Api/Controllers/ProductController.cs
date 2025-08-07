using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.DTOs;
using Domain.Models;
using Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador para gerenciar os produtos.
    /// </summary>
    [Route("api/product")]
    public class ProductController : BaseController<Product>
    {
        private readonly IProductService _productService;
        
        public ProductController(IProductService productService) : base(productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Cria um novo produto.
        /// </summary>
        /// <param name="productDto">Dados do produto a ser criado.</param>
        /// <returns>Retorna o produto criado.</returns>
        /// <response code="201">Produto criado com sucesso.</response>
        /// <response code="400">Se os dados do produto forem inválidos.</response>
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo produto.")]
        [SwaggerResponse(201, "Produto criado com sucesso.", typeof(ProductDTO))]
        [SwaggerResponse(400, "Dados inválidos. A quantidade deve ser maior que zero.", typeof(string))]
        
        public async Task<IActionResult> Create([FromBody] ProductDTO productDto)
        {
            if(productDto == null || productDto.QuantityAvaliable <= 0)
            {
                return BadRequest("Dados inválidos. A quantidade deve ser maior que zero.");
            }

            var createdProduct = await _productService.CreateProductAsync(productDto);
            var productDtoResponse = new ProductDTO
            {
                Id = createdProduct.Id,
                ProductName = createdProduct.ProductName,
                Price = createdProduct.Price,
                QuantityAvaliable = createdProduct.QuantityAvaliable
            };

            return CreatedAtAction(nameof(GetById), new { id = productDtoResponse.Id }, productDtoResponse);
        }
    }
}
