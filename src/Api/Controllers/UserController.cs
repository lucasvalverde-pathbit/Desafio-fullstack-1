using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;
using Application.DTOs;
using Common.DTOs;
using Domain.Models;
using Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador para gerenciar usuários.
    /// </summary>
    [Route("api/user")]
    public class UserController : BaseController<User>
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger) : base(userService)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Realiza o login do usuário.
        /// </summary>
        /// <param name="request">Objeto contendo as credenciais do usuário.</param>
        /// <returns>Retorna um token JWT se o login for bem-sucedido.</returns>
        /// <response code="200">Login bem-sucedido, retorna o token.</response>
        /// <response code="400">Se as credenciais forem inválidas.</response>
        [HttpPost("login")]
        [SwaggerOperation(Summary = "Realiza o login do usuário.")]
        [SwaggerResponse(200, "Login bem-sucedido, retorna o token.", typeof(object))]
        [SwaggerResponse(400, "Credenciais inválidas.", typeof(ResponseMessageDTO))]

        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _userService.LoginAsync(request.UserEmail, request.Password);
            return Ok(new { Token = token });
        }

        /// <summary>
        /// Realiza o cadastro de um novo usuário.
        /// </summary>
        /// <param name="request">Objeto contendo as informações do novo usuário.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Cadastro bem-sucedido.</response>
        /// <response code="400">Se houver erro no cadastro.</response>
        /// <response code="500">Erro interno do servidor.</response>
        [HttpPost("signup")]
        [SwaggerOperation(Summary = "Realiza o cadastro de um novo usuário.")]
        [SwaggerResponse(200, "Usuário cadastrado com sucesso.", typeof(ResponseMessageDTO))]
        [SwaggerResponse(400, "Erro no cadastro do usuário.", typeof(ResponseMessageDTO))]
        [SwaggerResponse(500, "Erro interno do servidor.", typeof(ResponseMessageDTO))]

        public async Task<IActionResult> Signup([FromBody] SignUpRequest request)
        {
            try
            {
                var userType = Enum.Parse<UserType>(request.UserType);
                await _userService.SignupAsync(request.Name, request.UserName, request.UserEmail, request.Password, userType);

                return Ok(new ResponseMessageDTO { Message = "Usuário cadastrado com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ResponseMessageDTO { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro durante o cadastro.");
                return StatusCode(500, new ResponseMessageDTO { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Atualiza as informações de um usuário existente.
        /// </summary>
        /// <param name="id">ID do usuário a ser atualizado.</param>
        /// <param name="userDTO">Objeto contendo os dados a serem atualizados.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Atualização bem-sucedida.</response>
        /// <response code="400">Erro ao atualizar o perfil do usuário.</response>
        [HttpPut("update/{id}")]
        [SwaggerOperation(Summary = "Atualiza as informações de um usuário existente.")]
        [SwaggerResponse(200, "Perfil atualizado com sucesso.", typeof(ResponseMessageDTO))]
        [SwaggerResponse(400, "Erro ao atualizar perfil.", typeof(ResponseMessageDTO))]
        
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDTO userDTO)
        {
            bool updated = await _userService.UpdateUserAsync(id, userDTO);
            if (!updated)
            {
                var response = new ResponseMessageDTO
                {
                    Message = "Erro ao atualizar perfil"
                };
                return BadRequest(response);
            }

            var successResponse = new ResponseMessageDTO
            {
                Message = "Perfil atualizado com sucesso"
            };
            return Ok(successResponse);
        }
    }
}