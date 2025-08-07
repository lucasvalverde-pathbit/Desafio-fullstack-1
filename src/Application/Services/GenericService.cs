//Serviços genéricos que são implementados pelos services das entidades
using Domain.Interfaces.Repositories;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        private readonly IGenericRepository<T> _repository;
        public GenericService(IGenericRepository<T> repository)
        {
            _repository = repository;
        }
        // Achar entidade por Id
        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }
        // Listar todos os dados inserirdos das entidades
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
        // Criar um dado de uma entidade
        public async Task CreateAsync(T entity)
        {
            await _repository.AddAsync(entity);
        }
        // Atualizar um dado de uma entidade
        public async Task UpdateAsync(T entity)
        {
            await _repository.UpdateAsync(entity);
        }
        // Deletar um dado de uma entidade
        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}