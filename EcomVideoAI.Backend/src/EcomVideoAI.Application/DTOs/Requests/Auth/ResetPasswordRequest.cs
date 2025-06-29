using System.ComponentModel.DataAnnotations;

namespace EcomVideoAI.Application.DTOs.Requests.Auth
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Token { get; set; }

        [Required]
        [MinLength(8)]
        public required string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword))]
        public required string ConfirmNewPassword { get; set; }
    }
} 