using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EcomVideoAI.Infrastructure.Services
{
    public class SocialAuthService : ISocialAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SocialAuthService> _logger;

        public SocialAuthService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<SocialAuthService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<SocialUserInfo?> ValidateGoogleTokenAsync(string accessToken, string? idToken = null)
        {
            try
            {
                // Use ID token if available (preferred), otherwise access token
                var tokenToValidate = !string.IsNullOrEmpty(idToken) ? idToken : accessToken;
                var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={tokenToValidate}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Google token validation failed: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(content);

                if (tokenInfo == null || string.IsNullOrEmpty(tokenInfo.Email))
                {
                    _logger.LogWarning("Invalid Google token response");
                    return null;
                }

                return new SocialUserInfo
                {
                    ProviderId = tokenInfo.Sub,
                    Provider = "Google",
                    Email = tokenInfo.Email,
                    FirstName = tokenInfo.GivenName,
                    LastName = tokenInfo.FamilyName,
                    AvatarUrl = tokenInfo.Picture,
                    EmailVerified = tokenInfo.EmailVerified.Equals("true", StringComparison.OrdinalIgnoreCase)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google token");
                return null;
            }
        }

        public async Task<SocialUserInfo?> ValidateFacebookTokenAsync(string accessToken)
        {
            try
            {
                var url = $"https://graph.facebook.com/me?fields=id,email,first_name,last_name,picture&access_token={accessToken}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Facebook token validation failed: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<FacebookUserInfo>(content);

                if (userInfo == null || string.IsNullOrEmpty(userInfo.Email))
                {
                    _logger.LogWarning("Invalid Facebook token response");
                    return null;
                }

                return new SocialUserInfo
                {
                    ProviderId = userInfo.Id,
                    Provider = "Facebook",
                    Email = userInfo.Email,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    AvatarUrl = userInfo.Picture?.Data?.Url,
                    EmailVerified = true // Facebook emails are generally verified
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Facebook token");
                return null;
            }
        }

        public async Task<SocialUserInfo?> ValidateAppleTokenAsync(string idToken, string? authCode = null)
        {
            try
            {
                // Apple Sign-In validation requires more complex JWT validation
                // For now, return a basic implementation
                // TODO: Implement proper Apple ID token validation with public keys
                _logger.LogWarning("Apple Sign-In validation is not fully implemented");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Apple token");
                return null;
            }
        }

        public async Task<SocialUserInfo?> ValidateTwitterTokenAsync(string accessToken)
        {
            try
            {
                // Twitter API v2 requires OAuth 2.0 and more complex validation
                // For now, return a basic implementation
                // TODO: Implement proper Twitter API validation
                _logger.LogWarning("Twitter validation is not fully implemented");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Twitter token");
                return null;
            }
        }

        // Helper classes for JSON deserialization
        private class GoogleTokenInfo
        {
            public string Sub { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string EmailVerified { get; set; } = string.Empty;
            public string GivenName { get; set; } = string.Empty;
            public string FamilyName { get; set; } = string.Empty;
            public string Picture { get; set; } = string.Empty;
        }

        private class FacebookUserInfo
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public FacebookPicture? Picture { get; set; }
        }

        private class FacebookPicture
        {
            public FacebookPictureData? Data { get; set; }
        }

        private class FacebookPictureData
        {
            public string Url { get; set; } = string.Empty;
        }
    }
} 