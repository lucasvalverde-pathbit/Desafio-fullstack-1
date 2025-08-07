// Interface que define a assinatura de métodos específicos de UserService
using Domain.Models;
using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces.Services
{
    public interface IUserService : IGenericService<User>
    {
        Task<bool> UpdateUserAsync(Guid userId, UpdateUserDTO userDTO);
        Task<string> LoginAsync(string userEmail, string password);
        Task SignupAsync(string name, string userName, string userEmail, string password, UserType userType);
    }
}
