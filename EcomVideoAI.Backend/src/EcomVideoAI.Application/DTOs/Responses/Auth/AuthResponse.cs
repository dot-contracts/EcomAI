namespace EcomVideoAI.Application.DTOs.Responses.Auth
{
    public class AuthResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required UserDto User { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Timezone { get; set; } = "UTC";
        public string Locale { get; set; } = "en-US";
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
} 