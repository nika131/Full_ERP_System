namespace NexusERP.Api.DTOs
{
    public class RegisterRequestDto
    {
        public string FullName { get; } = string.Empty;
        public string Username { get; } = string.Empty;
        public string Password { get; } = string.Empty;
        public string Role { get; } = "Cashier";
    }
}
