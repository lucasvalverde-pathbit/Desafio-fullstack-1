namespace Application.DTOs
{
    public class UpdateUserDTO
    {
        public required string Name { get; set; }
        public required string UserName { get; set; }
        public required string UserEmail { get; set; }
        public required string Password { get; set; }
    }
}