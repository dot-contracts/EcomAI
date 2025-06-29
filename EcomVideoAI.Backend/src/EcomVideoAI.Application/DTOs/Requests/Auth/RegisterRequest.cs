using System.ComponentModel.DataAnnotations;

namespace EcomVideoAI.Application.DTOs.Requests.Auth
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(8)]
        public required string Password { get; set; }

        [Required]
        [Compare(nameof(Password))]
        public required string ConfirmPassword { get; set; }

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Timezone { get; set; } = "UTC";

        public string? Locale { get; set; } = "en-US";

        public bool AcceptTerms { get; set; } = false;

        public bool AcceptMarketing { get; set; } = false;
    }
} 