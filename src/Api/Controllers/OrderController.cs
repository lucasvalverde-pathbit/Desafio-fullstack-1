using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Domain.Models;
using Domain.Enums;
using Application.DTOs;
using Application.Services;
using Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador para gerenciar as operações relacionadas aos pedidos.
    /// </summary>
    [Route("api/order")]
    public class OrderController : BaseController<Order>
    {
        private readonly IOrderService _orderService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserController> _logger;

        public OrderController(IOrderService orderService, HttpClient httpClient, ILogger<UserController> logger) : base(orderService)
        {
            _orderService = orderService;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Obtém o endereço formatado a partir do CEP fornecido.
        /// </summary>
        /// <param name="cep">O CEP para o qual o endereço será buscado.</param>
        /// <returns>Retorna o endereço formatado como uma string.</returns>
        /// <response code="200">Retorna o endereço formatado.</response>
        /// <response code="400">Se o CEP fornecido for inválido ou vazio.</response>
        /// <response code="500">Se ocorrer um erro ao consultar a API externa ou outro erro inesperado.</response>
        [Authorize(Policy = "Client")]
        [HttpGet("address/{cep}")]
        [SwaggerOperation(Summary = "Obtém o endereço formatado a partir do CEP fornecido.")]
        [SwaggerResponse(200, "Endereço encontrado e formatado com sucesso.", typeof(object))]
        [SwaggerResponse(400, "CEP inválido ou vazio.", typeof(ResponseMessageDTO))]
        [SwaggerResponse(500, "Erro ao buscar o endereço a partir do CEP.", typeof(ResponseMessageDTO))]
        
        public async Task<IActionResult> GetAddressFromCEP(string cep)
        {
            if (string.IsNullOrEmpty(cep))
            {
                return BadRequest("O CEP não pode ser vazio.");
            }

            try
            {
                var formattedAddress = await _orderService.GetAddressFromCEPAsync(cep);

                return Ok(new { address = formattedAddress });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = $"Erro ao buscar CEP: {ex.Message}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erro inesperado: {ex.Message}." });
            }
        }

        /// <summary>
        /// Obtém todos os pedidos de um cliente autenticado.
        /// </summary>
        /// <returns>Uma lista de pedidos do cliente.</returns>
        /// <response code="200">Retorna os pedidos do cliente.</response>
        /// <response code="401">Se o cliente não estiver autenticado.</response>
        /// <response code="404">Se não houver pedidos para o cliente.</response>
        [Authorize(Policy = "Client")]
        [HttpGet("client")]
        [SwaggerOperation(Summary = "Obtém todos os pedidos de um cliente autenticado.")]
        [SwaggerResponse(200, "Pedidos encontrados.", typeof(IEnumerable<Order>))]
        [SwaggerResponse(401, "Cliente não autenticado.", typeof(ResponseMessageDTO))]
        [SwaggerResponse(404, "Nenhum pedido encontrado.", typeof(ResponseMessageDTO))]

        public async Task<ActionResult<IEnumerable<Order>>> GetAllClient()
        {
            // Busca o Id
            var customerId = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

            if (string.IsNullOrEmpty(customerId) || !Guid.TryParse(customerId, out Guid id))
            {
                return Unauthorized(new ResponseMessageDTO { Message = "Cliente não autenticado." });
            }

            var orders = await _orderService.GetByCustomerIdAsync(id);
            
            if (orders == null || !orders.Any())
            {
                return NotFound(new ResponseMessageDTO { Message = "Nenhum pedido encontrado para este cliente." });
            }

            return Ok(orders);
        }
        

        /// <summary>
        /// Cria um novo pedido.
        /// </summary>
        /// <param name="request">Dados do pedido a ser criado.</param>
        /// <returns>Retorna o pedido criado.</returns>
        /// <response code="201">Retorna o pedido criado com sucesso.</response>
        /// <response code="400">Se os dados do pedido forem inválidos.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Cria um novo pedido.")]
        [SwaggerResponse(201, "Pedido criado com sucesso.", typeof(Order))]
        [SwaggerResponse(400, "Dados inválidos ou faltando informações no pedido.", typeof(ResponseMessageDTO))]

        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ResponseMessageDTO { Message = "Dados inválidos." });
            }

            if (request.Products.Count == 0)
            {
                return BadRequest(new ResponseMessageDTO { Message = "É obrigatório adicionar produtos ao pedido." });
            }

            if (request.Products.Count(c => c.Quantity <= 0 || c.Price <= 0) > 0)
            {
                return BadRequest(new ResponseMessageDTO { Message = "É obrigatório preencher as quantidades e o valor unitário de todos os produtos" });
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessageDTO { Message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém todos os pedidos de um administrador.
        /// </summary>
        /// <returns>Uma lista de todos os pedidos.</returns>
        /// <response code="200">Retorna todos os pedidos.</response>
        /// <response code="401">Se o usuário não for um administrador.</response>
        [Authorize(Policy = "Admin")]
        [HttpGet("admin")]
        [SwaggerOperation(Summary = "Obtém todos os pedidos de um administrador.")]
        [SwaggerResponse(200, "Pedidos encontrados.", typeof(IEnumerable<Order>))]
        [SwaggerResponse(401, "Acesso negado, usuário não é administrador.", typeof(ResponseMessageDTO))]

        public async Task<ActionResult<IEnumerable<Order>>> GetAllAdmin()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Atualiza o status de um pedido.
        /// </summary>
        /// <param name="id">ID do pedido a ser atualizado.</param>
        /// <param name="request">Novo status do pedido.</param>
        /// <returns>Resultado da operação de atualização do status.</returns>
        /// <response code="204">Status do pedido atualizado com sucesso.</response>
        /// <response code="400">Se o status fornecido for inválido.</response>
        /// <response code="500">Se ocorrer um erro no servidor ao tentar atualizar o status.</response>
        [Authorize(Policy = "Admin")]
        [HttpPut("{id}/status")]
        [SwaggerOperation(Summary = "Atualiza o status de um pedido.")]
        [SwaggerResponse(204, "Status atualizado com sucesso.")]
        [SwaggerResponse(400, "Status inválido.", typeof(ResponseMessageDTO))]
        [SwaggerResponse(500, "Erro ao atualizar status.", typeof(ResponseMessageDTO))]
        
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (!Enum.IsDefined(typeof(Status), request.Status))
            {
                return BadRequest(new ResponseMessageDTO { Message = "Status inválido." });
            }

            try
            {
                await _orderService.UpdateOrderStatusAsync(id, request.Status);
                return NoContent();
            } catch (Exception ex)
            {
                return StatusCode(500, new ResponseMessageDTO { Message = $"Erro ao atualizar status: {ex.Message}" });
            }
        }
    }
}