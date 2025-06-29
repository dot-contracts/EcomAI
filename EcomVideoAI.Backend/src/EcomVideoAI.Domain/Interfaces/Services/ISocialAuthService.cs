namespace EcomVideoAI.Domain.Interfaces.Services
{
    public interface ISocialAuthService
    {
        Task<SocialUserInfo?> ValidateGoogleTokenAsync(string accessToken, string? idToken = null);
        Task<SocialUserInfo?> ValidateFacebookTokenAsync(string accessToken);
        Task<SocialUserInfo?> ValidateAppleTokenAsync(string idToken, string? authCode = null);
        Task<SocialUserInfo?> ValidateTwitterTokenAsync(string accessToken);
    }

    public class SocialUserInfo
    {
        public required string ProviderId { get; set; }
        public required string Provider { get; set; }
        public required string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public bool EmailVerified { get; set; } = false;
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
} 