using EcomVideoAI.Application.DTOs.Requests.Auth;
using EcomVideoAI.Application.DTOs.Responses.Auth;
using EcomVideoAI.Application.DTOs.Responses.Common;
using EcomVideoAI.Application.Interfaces;
using EcomVideoAI.Domain.Entities;
using EcomVideoAI.Domain.Interfaces.Repositories;
using EcomVideoAI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EcomVideoAI.Application.UseCases.Auth
{
    public class SocialLoginUseCase : IAsyncUseCase<SocialLoginRequest, ApiResponse<AuthResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ISocialAuthService _socialAuthService;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogger<SocialLoginUseCase> _logger;

        public SocialLoginUseCase(
            IUserRepository userRepository,
            ISocialAuthService socialAuthService,
            IJwtService jwtService,
            IEmailService emailService,
            ILogger<SocialLoginUseCase> logger)
        {
            _userRepository = userRepository;
            _socialAuthService = socialAuthService;
            _jwtService = jwtService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponse>> ExecuteAsync(SocialLoginRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate terms acceptance for new users
                if (!request.AcceptTerms)
                {
                    return ApiResponse<AuthResponse>.ErrorResult("You must accept the terms and conditions to continue");
                }

                // Validate social provider token and get user info
                SocialUserInfo? socialUserInfo = null;

                switch (request.Provider.ToLowerInvariant())
                {
                    case "google":
                        socialUserInfo = await _socialAuthService.ValidateGoogleTokenAsync(request.AccessToken, request.IdToken);
                        break;
                    case "facebook":
                        socialUserInfo = await _socialAuthService.ValidateFacebookTokenAsync(request.AccessToken);
                        break;
                    case "apple":
                        socialUserInfo = await _socialAuthService.ValidateAppleTokenAsync(request.IdToken ?? request.AccessToken, request.AuthCode);
                        break;
                    case "twitter":
                        socialUserInfo = await _socialAuthService.ValidateTwitterTokenAsync(request.AccessToken);
                        break;
                    default:
                        return ApiResponse<AuthResponse>.ErrorResult($"Unsupported social provider: {request.Provider}");
                }

                if (socialUserInfo == null)
                {
                    _logger.LogWarning("Failed to validate {Provider} token", request.Provider);
                    return ApiResponse<AuthResponse>.ErrorResult("Failed to validate social login credentials");
                }

                // Use email from token or fallback to request
                var email = !string.IsNullOrEmpty(socialUserInfo.Email) 
                    ? socialUserInfo.Email 
                    : request.Email;

                if (string.IsNullOrEmpty(email))
                {
                    return ApiResponse<AuthResponse>.ErrorResult("Email is required for social login");
                }

                // Check if user already exists
                var existingUser = await _userRepository.GetByEmailAsync(email.ToLowerInvariant(), cancellationToken);
                User user;

                if (existingUser != null)
                {
                    // Existing user - update info if needed
                    user = existingUser;

                    // Check if account is locked or inactive
                    if (user.IsLocked)
                    {
                        _logger.LogWarning("Social login attempt on locked account: {Email}", email);
                        return ApiResponse<AuthResponse>.ErrorResult($"Account is locked until {user.LockedUntil:yyyy-MM-dd HH:mm} UTC");
                    }

                    if (!user.IsActive)
                    {
                        _logger.LogWarning("Social login attempt on inactive account: {Email}", email);
                        return ApiResponse<AuthResponse>.ErrorResult("Account is deactivated. Please contact support.");
                    }

                    // Update user info from social provider
                    if (!string.IsNullOrEmpty(socialUserInfo.FirstName) && string.IsNullOrEmpty(user.FirstName))
                        user.FirstName = socialUserInfo.FirstName;

                    if (!string.IsNullOrEmpty(socialUserInfo.LastName) && string.IsNullOrEmpty(user.LastName))
                        user.LastName = socialUserInfo.LastName;

                    if (!string.IsNullOrEmpty(socialUserInfo.AvatarUrl) && string.IsNullOrEmpty(user.AvatarUrl))
                        user.AvatarUrl = socialUserInfo.AvatarUrl;

                    // Mark email as verified if provider confirms it
                    if (socialUserInfo.EmailVerified && !user.EmailVerified)
                        user.VerifyEmail();

                    // Record successful login
                    user.RecordSuccessfulLogin();
                }
                else
                {
                    // New user - create account
                    user = new User(
                        email: email.ToLowerInvariant(),
                        passwordHash: "SOCIAL_LOGIN", // Placeholder - social users don't have passwords
                        firstName: socialUserInfo.FirstName ?? request.FirstName,
                        lastName: socialUserInfo.LastName ?? request.LastName)
                    {
                        AvatarUrl = socialUserInfo.AvatarUrl ?? request.AvatarUrl
                    };

                    // Mark email as verified if provider confirms it
                    if (socialUserInfo.EmailVerified)
                        user.VerifyEmail();

                    // Add user to database
                    await _userRepository.AddAsync(user, cancellationToken);
                    await _userRepository.SaveChangesAsync(cancellationToken);

                    // Add default user role
                    var userRole = new UserRole(user.Id, "User");
                    user.Roles.Add(userRole);

                    // Create default preferences
                    var preferences = new UserPreferences(user.Id)
                    {
                        MarketingEmailsEnabled = request.AcceptMarketing
                    };
                    user.Preferences = preferences;

                    // Create default notification settings
                    var notificationSettings = new NotificationSettings(user.Id);
                    user.NotificationSettings = notificationSettings;

                    _logger.LogInformation("New user created via {Provider}: {Email}", request.Provider, email);

                    // Send welcome email (don't await to not block login)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName ?? "User");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
                        }
                    }, cancellationToken);
                }

                await _userRepository.SaveChangesAsync(cancellationToken);

                // Get user with roles
                var userWithRoles = await _userRepository.GetWithRolesAsync(user.Id, cancellationToken);
                if (userWithRoles == null)
                {
                    return ApiResponse<AuthResponse>.ErrorResult("User data could not be retrieved");
                }

                // Get user roles
                var roles = userWithRoles.Roles.Select(r => r.RoleName).ToList();

                // Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(userWithRoles, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Save refresh token (longer expiry for social logins)
                var refreshTokenEntity = new RefreshToken(
                    userWithRoles.Id,
                    refreshToken,
                    DateTime.UtcNow.AddDays(30));

                userWithRoles.RefreshTokens.Add(refreshTokenEntity);
                await _userRepository.SaveChangesAsync(cancellationToken);

                var userDto = new UserDto
                {
                    Id = userWithRoles.Id,
                    Email = userWithRoles.Email,
                    EmailVerified = userWithRoles.EmailVerified,
                    FirstName = userWithRoles.FirstName,
                    LastName = userWithRoles.LastName,
                    AvatarUrl = userWithRoles.AvatarUrl,
                    PhoneNumber = userWithRoles.PhoneNumber,
                    DateOfBirth = userWithRoles.DateOfBirth,
                    Timezone = userWithRoles.Timezone,
                    Locale = userWithRoles.Locale,
                    IsActive = userWithRoles.IsActive,
                    LastLoginAt = userWithRoles.LastLoginAt,
                    CreatedAt = userWithRoles.CreatedAt,
                    Roles = roles
                };

                var authResponse = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = userDto,
                    ExpiresAt = _jwtService.GetTokenExpiration(accessToken)
                };

                _logger.LogInformation("User logged in successfully via {Provider}: {Email}", request.Provider, userWithRoles.Email);

                return ApiResponse<AuthResponse>.SuccessResult(authResponse, 
                    existingUser != null ? "Login successful" : "Account created and login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during {Provider} social login for email: {Email}", 
                    request.Provider, request.Email);
                return ApiResponse<AuthResponse>.ErrorResult("An error occurred during social login. Please try again.");
            }
        }
    }
} 