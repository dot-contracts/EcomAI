using EcomVideoAI.Application.DTOs.Requests.Auth;
using EcomVideoAI.Application.DTOs.Responses.Auth;
using EcomVideoAI.Application.DTOs.Responses.Common;
using EcomVideoAI.Application.UseCases.Auth;
using EcomVideoAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcomVideoAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterUseCase _registerUseCase;
        private readonly LoginUseCase _loginUseCase;
        private readonly SocialLoginUseCase _socialLoginUseCase;
        private readonly RefreshTokenUseCase _refreshTokenUseCase;
        private readonly LogoutUseCase _logoutUseCase;
        private readonly ForgotPasswordUseCase _forgotPasswordUseCase;
        private readonly ResetPasswordUseCase _resetPasswordUseCase;
        private readonly GetCurrentUserUseCase _getCurrentUserUseCase;
        private readonly VerifyEmailUseCase _verifyEmailUseCase;
        private readonly ResendVerificationUseCase _resendVerificationUseCase;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            RegisterUseCase registerUseCase,
            LoginUseCase loginUseCase,
            SocialLoginUseCase socialLoginUseCase,
            RefreshTokenUseCase refreshTokenUseCase,
            LogoutUseCase logoutUseCase,
            ForgotPasswordUseCase forgotPasswordUseCase,
            ResetPasswordUseCase resetPasswordUseCase,
            GetCurrentUserUseCase getCurrentUserUseCase,
            VerifyEmailUseCase verifyEmailUseCase,
            ResendVerificationUseCase resendVerificationUseCase,
            ILogger<AuthController> logger)
        {
            _registerUseCase = registerUseCase;
            _loginUseCase = loginUseCase;
            _socialLoginUseCase = socialLoginUseCase;
            _refreshTokenUseCase = refreshTokenUseCase;
            _logoutUseCase = logoutUseCase;
            _forgotPasswordUseCase = forgotPasswordUseCase;
            _resetPasswordUseCase = resetPasswordUseCase;
            _getCurrentUserUseCase = getCurrentUserUseCase;
            _verifyEmailUseCase = verifyEmailUseCase;
            _resendVerificationUseCase = resendVerificationUseCase;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user with email and password
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 400)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<AuthResponse>.ErrorResult(errors));
            }

            var result = await _registerUseCase.ExecuteAsync(request, cancellationToken);
            
            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return Ok(result);
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 400)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 401)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<AuthResponse>.ErrorResult(errors));
            }

            var result = await _loginUseCase.ExecuteAsync(request, cancellationToken);
            
            if (!result.Success)
            {
                if (result.Errors.Any(e => e.Contains("Invalid email or password") || e.Contains("locked") || e.Contains("deactivated")))
                    return Unauthorized(result);
                
                return BadRequest(result);
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(result);
        }

        /// <summary>
        /// Login or register with social providers (Google, Facebook, Apple, Twitter)
        /// </summary>
        [HttpPost("social-login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 400)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 401)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> SocialLogin(
            [FromBody] SocialLoginRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<AuthResponse>.ErrorResult(errors));
            }

            var result = await _socialLoginUseCase.ExecuteAsync(request, cancellationToken);
            
            if (!result.Success)
            {
                if (result.Errors.Any(e => e.Contains("Failed to validate") || e.Contains("locked") || e.Contains("deactivated")))
                    return Unauthorized(result);
                
                return BadRequest(result);
            }

            _logger.LogInformation("User logged in successfully via {Provider}: {Email}", 
                request.Provider, request.Email);
            
            return Ok(result);
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 400)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 401)]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<AuthResponse>.ErrorResult(errors));
            }

            var result = await _refreshTokenUseCase.ExecuteAsync(request, cancellationToken);
            
            if (!result.Success)
            {
                if (result.Errors.Any(e => e.Contains("Invalid") || e.Contains("expired") || e.Contains("revoked")))
                    return Unauthorized(result);
                
                return BadRequest(result);
            }

            _logger.LogInformation("Token refreshed successfully");
            return Ok(result);
        }

        /// <summary>
        /// Logout user and revoke refresh token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse>> Logout(
            [FromBody] LogoutRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse.ErrorResult(errors));
            }

            var result = await _logoutUseCase.ExecuteAsync(request, cancellationToken);
            
            _logger.LogInformation("User logged out successfully");
            return Ok(result);
        }

        /// <summary>
        /// Request password reset token
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse>> ForgotPassword(
            [FromBody] ForgotPasswordRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse.ErrorResult(errors));
            }

            var result = await _forgotPasswordUseCase.ExecuteAsync(request, cancellationToken);
            
            return Ok(result);
        }

        /// <summary>
        /// Reset password using reset token
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse>> ResetPassword(
            [FromBody] ResetPasswordRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse.ErrorResult(errors));
            }

            var result = await _resetPasswordUseCase.ExecuteAsync(request, cancellationToken);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 401)]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser(CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Invalid or missing user_id claim in token");
                return Unauthorized(ApiResponse<UserDto>.ErrorResult("Invalid authentication token"));
            }

            var result = await _getCurrentUserUseCase.ExecuteAsync(userId, cancellationToken);
            
            if (!result.Success)
            {
                if (result.Errors.Any(e => e.Contains("not found") || e.Contains("deactivated")))
                    return Unauthorized(result);
                    
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Verify email address using verification token
        /// </summary>
        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse>> VerifyEmail(
            [FromBody] VerifyEmailRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse.ErrorResult(errors));
            }

            var result = await _verifyEmailUseCase.ExecuteAsync(request, cancellationToken);
            
            if (!result.Success)
                return BadRequest(result);
                
            return Ok(result);
        }

        /// <summary>
        /// Resend email verification
        /// </summary>
        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse>> ResendVerification(
            [FromBody] ResendVerificationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse.ErrorResult(errors));
            }

            var result = await _resendVerificationUseCase.ExecuteAsync(request, cancellationToken);
            
            return Ok(result);
        }
    }
} 