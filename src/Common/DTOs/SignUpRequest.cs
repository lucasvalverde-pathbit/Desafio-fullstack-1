using Domain.Enums;

namespace Common.DTOs
{
public class SignUpRequest

    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string Password { get; set; }        
        public string UserType { get; set; }
    }
}
