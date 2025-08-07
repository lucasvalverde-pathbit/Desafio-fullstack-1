using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers
{
    /// <summary>
    /// Controlador base genérico que fornece operações CRUD para qualquer entidade.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade.</typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController<T> : ControllerBase where T : class
    {
        protected readonly IGenericService<T> _service;

        public BaseController(IGenericService<T> service)
        {
            _service = service;
        }

        /// <summary>
        /// Retorna todas as entidades.
        /// </summary>
        /// <returns>Lista de entidades.</returns>
        /// <response code="200">Retorna a lista de entidades.</response>
        /// <response code="500">Erro interno ao buscar as entidades.</response>
        [HttpGet]
        [SwaggerOperation(Summary = "Retorna todas as entidades", Description = "Obtém todas as entidades armazenadas.")]
        [SwaggerResponse(200, "Lista de entidades retornada com sucesso.", typeof(IEnumerable<object>))]
        [SwaggerResponse(500, "Erro ao tentar recuperar as entidades.")]
        public virtual async Task<ActionResult<IEnumerable<T>>> GetAll()
        {
            var entities = await _service.GetAllAsync();
            return Ok(entities);
        }

        /// <summary>
        /// Retorna uma entidade pelo ID.
        /// </summary>
        /// <param name="id">ID da entidade.</param>
        /// <returns>Entidade com o ID informado.</returns>
        /// <response code="200">Retorna a entidade encontrada.</response>
        /// <response code="404">Se a entidade com o ID não for encontrada.</response>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Retorna uma entidade pelo ID", Description = "Obtém uma entidade específica pelo seu ID.")]
        [SwaggerResponse(200, "Entidade retornada com sucesso.", typeof(object))]
        [SwaggerResponse(404, "Entidade não encontrada com o ID fornecido.")]
        public virtual async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            return NotFound($"Id {id} não encontrado");
        }

        /// </summary>
        /// <param name="id">ID da entidade a ser atualizada.</param>
        /// <param name="entity">Entidade com os novos dados.</param>
        /// <returns>Status da operação.</returns>
        /// <response code="204">Se a atualização for bem-sucedida.</response>
        /// <response code="400">Se o ID for inválido ou não corresponder ao da entidade.</response>
        /// <response code="404">Se a entidade com o ID não for encontrada.</response>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualiza uma entidade", Description = "Atualiza os dados de uma entidade existente.")]
        [SwaggerResponse(204, "Entidade atualizada com sucesso.")]
        [SwaggerResponse(400, "O ID fornecido é inválido ou não corresponde ao da entidade.")]
        [SwaggerResponse(404, "Entidade não encontrada com o ID fornecido.")]
        public async Task<IActionResult> Update(Guid id, [FromBody] T entity)
        {
            if (entity is null || entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString() != id.ToString())
            {
                return BadRequest($"Id {id} inválido");
            }

            await _service.UpdateAsync(entity);
            return NoContent();
        }

        /// <summary>
        /// Exclui uma entidade pelo ID.
        /// </summary>
        /// <param name="id">ID da entidade a ser excluída.</param>
        /// <returns>Status da operação.</returns>
        /// <response code="204">Se a exclusão for bem-sucedida.</response>
        /// <response code="404">Se a entidade com o ID não for encontrada.</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Exclui uma entidade", Description = "Remove uma entidade do banco de dados pelo seu ID.")]
        [SwaggerResponse(204, "Entidade excluída com sucesso.")]
        [SwaggerResponse(404, "Entidade não encontrada com o ID fornecido.")]
        
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity is null)
            {
                return NotFound($"Id {id} não encontrado");
            }

            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}