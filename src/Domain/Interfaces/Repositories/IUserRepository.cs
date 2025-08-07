// Assinatura de métodos específicos que serão implementados em UserRepository
using Domain.Models;
using Domain.Enums;

namespace Domain.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByIdAsync(Guid id);
        Task UpdateAsync(User user);
        Task<User?> GetByEmailAsync(string userEmail);
        Task<User> SignupAsync(string name, string userName, string userEmail, string password, UserType userType);
    }
}
