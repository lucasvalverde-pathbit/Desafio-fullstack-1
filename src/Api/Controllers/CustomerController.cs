using Microsoft.AspNetCore.Mvc;
using Domain.Models;
using Application.DTOs;
using Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador para gerenciar as operações relacionadas aos clientes.
    /// </summary>
    [Route("api/customer")]
    public class CustomerController : BaseController<Customer>
    {
        private readonly ICustomerService _customerService;
        private readonly IUserService _userService;

        public CustomerController(ICustomerService customerService, IUserService userService) : base(customerService)
        {
            _customerService = customerService;
            _userService = userService;
        }

        /// <summary>
        /// Criação de cliente. 
        /// Clientes são criados automaticamente ao cadastrar um usuário do tipo 'Cliente'.
        /// </summary>
        /// <returns>Mensagem informando que o cliente é criado automaticamente ao cadastrar um usuário.</returns>
        /// <response code="400">Se houver erro ao tentar criar o cliente.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Cria um cliente automaticamente ao cadastrar um usuário do tipo 'Cliente'.")]
        [SwaggerResponse(400, "Erro ao tentar criar cliente.", typeof(ResponseMessageDTO))]

        public IActionResult Create()
        {
            var responseMessageDTO = new ResponseMessageDTO
            {
                Message = "Clientes são criados automaticamente ao cadastrar um usuário do tipo 'Cliente'."
            };
            
            return BadRequest(responseMessageDTO);
        }

        /// <summary>
        /// Atualiza o perfil do usuário.
        /// </summary>
        /// <param name="id">ID do usuário a ser atualizado.</param>
        /// <param name="userDTO">Dados de atualização do usuário.</param>
        /// <returns>Mensagem informando o sucesso ou falha na atualização do perfil.</returns>
        /// <response code="200">Se o perfil foi atualizado com sucesso.</response>
        /// <response code="400">Se houve erro ao tentar atualizar o perfil.</response>
        [HttpPut("update/{id}")]
        [SwaggerOperation(Summary = "Atualiza o perfil de um usuário", Description = "Atualiza as informações do usuário com base no ID fornecido.")]
        [SwaggerResponse(200, "Perfil atualizado com sucesso.")]
        [SwaggerResponse(400, "Erro ao atualizar o perfil.")]
        
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDTO userDTO)
        {
            bool updated = await _userService.UpdateUserAsync(id, userDTO);
            if (!updated) return BadRequest("Erro ao atualizar perfil");

            return Ok("Perfil atualizado com sucesso");
        }
    }
}
