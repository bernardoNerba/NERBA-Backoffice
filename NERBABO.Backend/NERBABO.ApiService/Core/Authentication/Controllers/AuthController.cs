using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Authentication.Dtos;
using NERBABO.ApiService.Core.Authentication.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IJwtService jwtService,
        IRoleService roleService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly IRoleService _roleService = roleService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Refreshes the authentication token for the currently logged-in user.
        /// </summary>
        /// <response code="200">Returns the user DTO with refreshed token information</response>
        /// <response code="404">If the token is invalid or user cannot be found</response>
        /// <response code="500">Unexpected error occurred.</response>
        [Authorize(Policy = "ActiveUser")]
        [HttpGet("refresh-user-token")]
        public async Task<IActionResult> RefreshUserToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException();
           
            Result<LoggedInUserDto> result = await _jwtService.GenerateRefreshTokenAsync(userId);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Authenticates a user by username/email and password.
        /// </summary>
        /// <param name="model">The LoginDto object containing username/email and password.</param>
        /// <response code="200">Returns the authenticated user's details with the issued jwt.</response>
        /// <response code="400">Invalid username/email or password.</response>
        /// <response code="401">User is blocked or not authorized.</response>
        /// <response code="404">User not found.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            Result<LoggedInUserDto> result = await _jwtService.GenerateJwtOnLoginAsync(model);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Logs out the current user by invalidating their JWT token.
        /// </summary>
        /// <response code="200">Token successfully invalidated, user logged out</response>
        /// <response code="401">User is not authenticated or token is invalid</response>
        /// <response code="500">Unexpected error occurred.</response>
        [Authorize(Policy = "ActiveUser")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Get the token from the Authorization header
            var authHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "No token provided" });
            }

            var token = authHeader["Bearer ".Length..].Trim();

            Result result = await _jwtService.InvalidateTokenAsync(token);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Sets a role to a user.
        /// </summary>
        /// <param name="userRole">The UserRoleDto object containing user ID and roles to be assigned.</param>
        /// <response code="200">Role updated successfully.</response>
        /// <response code="400">Validation ERROR while performing validation on not allowing to assing the Admin role to a blocked user,
        /// removing or adding roles that do not exist, or updating the user instance.
        /// </response>
        /// <response code="404">User, one or more role not found.</response>
        /// <response code="401">User is not authorized.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        [HttpPost("set-role")]
        public async Task<IActionResult> SetRoleToUserAsync([FromBody] UserRoleDto userRole)
        {
            Result result = await _roleService.UpdateUserRolesAsync(userRole);
            return _responseHandler.HandleResult(result);
        }
    }
}
