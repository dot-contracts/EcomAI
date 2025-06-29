using System.ComponentModel.DataAnnotations;

namespace EcomVideoAI.Application.DTOs.Requests.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public required string RefreshToken { get; set; }
    }
} 