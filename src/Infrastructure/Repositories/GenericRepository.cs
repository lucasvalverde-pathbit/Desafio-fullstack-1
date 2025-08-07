// Repositório genérico para funções CRUD

using Microsoft.EntityFrameworkCore;
using Domain.Interfaces.Repositories;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public readonly AppDbContext _context; // Referência a classe AppDbContext

        public GenericRepository(AppDbContext context) // Construtor
        {
            _context = context;
        }

        // Listar registros
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        // Listar por id
        public async Task<T> GetByIdAsync(Guid id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if(entity is null)
            {
                throw new KeyNotFoundException($"Id {id} não encontrado.");
            }
            return entity;
        }

        // Adicionar novo
        public async Task AddAsync(T entity) 
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Atualizar
        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        } 
        
        // Excluir
        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if(entity is not null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}