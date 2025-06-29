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
    public class RegisterUseCase : IAsyncUseCase<RegisterRequest, ApiResponse<AuthResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegisterUseCase> _logger;

        public RegisterUseCase(
            IUserRepository userRepository,
            IEmailVerificationTokenRepository emailVerificationTokenRepository,
            IPasswordService passwordService,
            IJwtService jwtService,
            IEmailService emailService,
            ILogger<RegisterUseCase> logger)
        {
            _userRepository = userRepository;
            _emailVerificationTokenRepository = emailVerificationTokenRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponse>> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate terms acceptance
                if (!request.AcceptTerms)
                {
                    return ApiResponse<AuthResponse>.ErrorResult("You must accept the terms and conditions to register");
                }

                // Check if user already exists
                var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponse>.ErrorResult("An account with this email address already exists");
                }

                // Hash password
                var passwordHash = _passwordService.HashPassword(request.Password);

                // Create user
                var user = new User(
                    email: request.Email.ToLowerInvariant(),
                    passwordHash: passwordHash,
                    firstName: request.FirstName,
                    lastName: request.LastName)
                {
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = request.DateOfBirth,
                    Timezone = request.Timezone ?? "UTC",
                    Locale = request.Locale ?? "en-US"
                };

                // Add user to database
                await _userRepository.AddAsync(user, cancellationToken);

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

                await _userRepository.SaveChangesAsync(cancellationToken);

                // Generate email verification token
                var verificationToken = _passwordService.GenerateSecureToken(32);
                
                // Create and store the verification token (expires in 24 hours)
                var emailVerificationToken = new EmailVerificationToken(
                    user.Id,
                    verificationToken,
                    DateTime.UtcNow.AddHours(24));

                await _emailVerificationTokenRepository.AddAsync(emailVerificationToken, cancellationToken);
                await _emailVerificationTokenRepository.SaveChangesAsync(cancellationToken);

                // Generate tokens
                var roles = new List<string> { "User" };
                var accessToken = _jwtService.GenerateAccessToken(user, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Save refresh token
                var refreshTokenEntity = new RefreshToken(
                    user.Id,
                    refreshToken,
                    DateTime.UtcNow.AddDays(30));

                user.RefreshTokens.Add(refreshTokenEntity);
                await _userRepository.SaveChangesAsync(cancellationToken);

                // Send verification email (don't await to not block registration)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmailVerificationAsync(user.Email, user.FirstName ?? "User", verificationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send verification email to {Email}", user.Email);
                    }
                }, cancellationToken);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    EmailVerified = user.EmailVerified,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AvatarUrl = user.AvatarUrl,
                    PhoneNumber = user.PhoneNumber,
                    DateOfBirth = user.DateOfBirth,
                    Timezone = user.Timezone,
                    Locale = user.Locale,
                    IsActive = user.IsActive,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt,
                    Roles = roles
                };

                var authResponse = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = userDto,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Access token expires in 60 minutes
                };

                _logger.LogInformation("User registered successfully: {Email}", user.Email);

                return ApiResponse<AuthResponse>.SuccessResult(authResponse, "Registration successful! Please check your email to verify your account.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration for email: {Email}", request.Email);
                return ApiResponse<AuthResponse>.ErrorResult("An error occurred during registration. Please try again.");
            }
        }
    }
} 