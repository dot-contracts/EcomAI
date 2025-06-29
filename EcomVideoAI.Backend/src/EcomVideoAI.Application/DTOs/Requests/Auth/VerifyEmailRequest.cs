using System.ComponentModel.DataAnnotations;

namespace EcomVideoAI.Application.DTOs.Requests.Auth
{
    public class VerifyEmailRequest
    {
        [Required]
        public required string Token { get; set; }
    }
} 