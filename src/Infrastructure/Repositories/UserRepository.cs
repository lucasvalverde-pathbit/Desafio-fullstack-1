using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;
using Domain.Models;
using Domain.Interfaces.Repositories;
using Infrastructure.Persistence;
using Domain.Enums;

namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        // Construtor para receber o AppDbContext e passar pra base GenericRepository
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        // Método implementado para encontrar um usuário pelo email
        public async Task<User?> GetByEmailAsync(string userEmail)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
        }

        // Método implementado para atualizar uma informação do usuário
        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // Implementação do método de Cadastrar
        public async Task<User> SignupAsync(string name, string userName, string userEmail, string password, UserType userType)
        {
            using var sha256 = SHA256.Create();
            var hashedPassword = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));

            if (await _context.Users.AnyAsync(u => u.UserEmail == userEmail))
            {
                throw new InvalidOperationException("E-mail já cadastrado.");
            }

            var user = new User
            {
                Name = name,
                UserName = userName,
                UserEmail = userEmail,
                Password = hashedPassword,
                UserType = userType,
            };
            
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
