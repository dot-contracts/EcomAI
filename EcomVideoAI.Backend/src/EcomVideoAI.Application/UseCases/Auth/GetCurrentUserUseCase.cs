using EcomVideoAI.Application.DTOs.Requests.Auth;
using EcomVideoAI.Application.DTOs.Responses.Auth;
using EcomVideoAI.Application.DTOs.Responses.Common;
using EcomVideoAI.Application.Interfaces;
using EcomVideoAI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EcomVideoAI.Application.UseCases.Auth
{
    public class GetCurrentUserUseCase : IAsyncUseCase<GetCurrentUserRequest, ApiResponse<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetCurrentUserUseCase> _logger;

        public GetCurrentUserUseCase(
            IUserRepository userRepository,
            ILogger<GetCurrentUserUseCase> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<UserDto>> ExecuteAsync(GetCurrentUserRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Note: In a real implementation, you would get the userId from the JWT token claims
                // For now, we'll need to modify the controller to pass the userId from the JWT
                // This is a placeholder implementation
                
                return ApiResponse<UserDto>.ErrorResult("User ID must be provided from JWT token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current user");
                return ApiResponse<UserDto>.ErrorResult("An error occurred while retrieving user information");
            }
        }

        // Helper method for controller to call with userId from JWT
        public async Task<ApiResponse<UserDto>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetWithRolesAsync(userId, cancellationToken);
                
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return ApiResponse<UserDto>.ErrorResult("User not found");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user attempted to get profile: {UserId}", userId);
                    return ApiResponse<UserDto>.ErrorResult("Account is deactivated");
                }

                var roles = user.Roles.Select(r => r.RoleName).ToList();

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

                return ApiResponse<UserDto>.SuccessResult(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current user: {UserId}", userId);
                return ApiResponse<UserDto>.ErrorResult("An error occurred while retrieving user information");
            }
        }
    }
} 