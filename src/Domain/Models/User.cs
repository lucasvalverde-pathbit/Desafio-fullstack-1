using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string UserName { get; set; }
        public required string UserEmail { get; set; }
        public required string Password { get; set; }
        [Column(TypeName = "text")]
        public UserType UserType { get; set; }
        
    }
}