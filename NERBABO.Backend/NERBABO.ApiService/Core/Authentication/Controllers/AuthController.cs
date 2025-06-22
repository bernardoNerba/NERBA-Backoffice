using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Authentication.Dtos;
using NERBABO.ApiService.Core.Authentication.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        UserManager<User> userManager,
        IJwtService jwtService,
        IRoleService roleService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IRoleService _roleService = roleService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Refreshes the authentication token for the currently logged-in user.
        /// </summary>
        /// <remarks>
        /// This endpoint requires authorization and will return a new user DTO containing
        /// the current user's information which can be used to refresh client-side tokens.
        /// If the user cannot be found (invalid token), returns a 400 Bad Request response.
        /// </remarks>
        /// <returns>
        /// Returns 200 OK with the user DTO if successful.
        /// Returns 400 Bad Request if the user token is invalid or user not found.
        /// </returns>
        /// <response code="200">Returns the user DTO with refreshed token information</response>
        /// <response code="400">If the token is invalid or user cannot be found</response>
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
        /// <param name="model">
        /// The login request containing:
        /// - <see cref="LoginDto.UsernameOrEmail"/> (can be either username or email)
        /// - <see cref="LoginDto.Password"/> (user's password)
        /// </param>
        /// <returns>
        /// Returns an <see cref="ActionResult"/> with the following possible outcomes:
        /// - <see cref="BadRequestResult"/> (400) if username/email or password is invalid.
        /// - <see cref="OkObjectResult"/> (200) with the authenticated user's details (<see cref="ApplicationUserDto"/>) if login succeeds.
        /// </returns>
        /// <remarks>
        /// This endpoint performs the following steps:
        /// 1. Attempts to find the user by username, then falls back to email if not found.
        /// 2. Validates the provided password against the user's stored credentials.
        /// 3. Returns the user's details (excluding sensitive data) upon successful authentication.
        /// 
        /// Security Note:
        /// - Returns the same generic error message ("Email/Username ou password inválidos") for both invalid credentials and non-existent users to avoid enumeration attacks.
        /// - Uses <see cref="SignInManager{TUser}.CheckPasswordSignInAsync"/> for secure password validation.
        /// </remarks>
        /// <response code="200">Returns the authenticated user's details.</response>
        /// <response code="400">Invalid username/email or password.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            Result<LoggedInUserDto> result = await _jwtService.GenerateJwtOnLoginAsync(model);
            return _responseHandler.HandleResult(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("set-role")]
        public async Task<IActionResult> SetRoleToUserAsync([FromBody] UserRoleDto userRole)
        {
            Result result = await _roleService.UpdateUserRolesAsync(userRole);
            return _responseHandler.HandleResult(result);
        }
    }
}
