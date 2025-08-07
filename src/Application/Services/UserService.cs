using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs;
using Application.Interfaces.Services;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services
{
    public class UserService : GenericService<User>, IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomerService _customerService;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _jwtKey;

        public UserService(
            IUserRepository userRepository, 
            ICustomerService customerService, 
            ILogger<UserService> logger, 
            IConfiguration configuration
        ) : base(userRepository)
        {
            _userRepository = userRepository;
            _customerService = customerService;
            _configuration = configuration;
            _logger = logger;
            _jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurado.");
        }

        // Método para atualizar informações de usuário
        public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDTO userDTO)
        {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return false;

                user.Name = userDTO.Name;
                user.UserName = userDTO.UserName;
                user.UserEmail = userDTO.UserEmail;
                if (!string.IsNullOrWhiteSpace(userDTO.Password))
                {
                    using var sha256 = SHA256.Create();
                    user.Password = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(userDTO.Password)));
                }

                await _userRepository.UpdateAsync(user);

                return true;
        }

        // Método para fazer login
        public async Task<string> LoginAsync(string userEmail, string password)
        {
            using var sha256 = SHA256.Create();
            var hashedPassword = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            _logger.LogInformation("Tentando fazer login com o usuário: {UserEmail}", userEmail);

            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user is null || user.Password != hashedPassword)
            {
                _logger.LogWarning("Usuário não encontrado: {UserEmail}", userEmail);
                throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
            }

            // UserType para string
            var userTypeString = user.UserType.ToString();
            return GenerateJwtToken(user, userTypeString);
        }

        // Cadastrar um usuário
        public async Task SignupAsync(string name, string userName, string userEmail, string password, UserType userType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome deve ser preenchido.");
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("O nome de usuário deve ser preenchido.");
            if (string.IsNullOrWhiteSpace(userEmail))
                throw new ArgumentException("O email deve ser preenchido.");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("A senha deve ser preenchida.");
            if (userType != UserType.Administrador && userType != UserType.Cliente)
                throw new ArgumentException("Tipo de usuário inválido.");

            using var sha256 = SHA256.Create();
            var hashedPassword = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            var existingUser = await _userRepository.GetByEmailAsync(userEmail);
            if (existingUser is not null)
            {
                throw new InvalidOperationException("Email já cadastrado");
            }

            var user = new User
            {
                Name = name,
                UserName = userName,
                UserEmail = userEmail,
                Password = hashedPassword,
                UserType = userType,
            };

            await _userRepository.AddAsync(user);

            if (userType == UserType.Cliente)
            {
                var customer = new Customer
                {
                    Id = user.Id,
                    CustomerName = user.Name,
                    CustomerEmail = user.UserEmail,
                };

                await _customerService.CreateAsync(customer);
            }
        }

        // Gerar token JWT
        public string GenerateJwtToken(User user, string userType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);
            var expirationTime = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("name", user.Name),
                new Claim("userName", user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.UserEmail),
                new Claim("userType", userType),
                new Claim(JwtRegisteredClaimNames.Exp, expirationTime.ToString())  
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}