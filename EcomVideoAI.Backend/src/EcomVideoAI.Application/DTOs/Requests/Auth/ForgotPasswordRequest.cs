using System.ComponentModel.DataAnnotations;

namespace EcomVideoAI.Application.DTOs.Requests.Auth
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
} 