using System.ComponentModel.DataAnnotations;

namespace EcomVideoAI.Application.DTOs.Requests.Auth
{
    public class SocialLoginRequest
    {
        [Required]
        public required string Provider { get; set; } // "Google", "Facebook", "Apple", "Twitter"

        [Required]
        public required string AccessToken { get; set; }

        public string? IdToken { get; set; } // For Apple/Google

        public string? AuthCode { get; set; } // For Apple

        public string? Email { get; set; } // Fallback if not in token

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? AvatarUrl { get; set; }

        public bool AcceptTerms { get; set; } = false;

        public bool AcceptMarketing { get; set; } = false;
    }
} 