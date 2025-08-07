namespace Application.DTOs
{
    public class LoginRequest
    {
        public required string UserEmail { get; set; }
        public required string Password { get; set; }
    }
}
