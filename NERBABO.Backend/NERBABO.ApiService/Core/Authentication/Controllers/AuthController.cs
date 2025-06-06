using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Authentication.Dtos;
using NERBABO.ApiService.Core.Authentication.Services;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IRoleService _roleService;

        public AuthController(ILogger<AuthController> logger,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            AppDbContext context,
            IJwtService jwtService,
            IRoleService roleService)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _jwtService = jwtService;
            _roleService = roleService;
        }
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
        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult> RefreshUserToken()
        {
            var userToBuildJWT = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");

            if (userToBuildJWT == null)
            {
                _logger.LogWarning("User not found for token refresh. Invalid token or user not found.");
                return BadRequest("Token Inválido. Porfavor efetue login para obter um novo token");
            }

            var loogedUser = new LoggedInUserDto(
                userToBuildJWT.Person!.FirstName,
                userToBuildJWT.Person!.LastName,
                _jwtService.CreateJwt(userToBuildJWT).Result
            );
            _logger.LogInformation("Token refreshed successfully for {UserId}", userToBuildJWT.Id);
            return Ok(loogedUser);
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
        public async Task<ActionResult> Login(LoginDto model)
        {
            // Try to find user by username first, then fall back to email
            var user = await _userManager.FindByNameAsync(model.UsernameOrEmail)
                ?? await _userManager.FindByEmailAsync(model.UsernameOrEmail);

            if (user == null)
            {
                _logger.LogWarning("Login attempt failed for {UsernameOrEmail}. User not found.", model.UsernameOrEmail);
                return BadRequest("Email/Username ou password inválidos.");
            }

            // Verify password
            var result = await _signInManager
                .CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Login attempt failed for {UsernameOrEmail}. User not found.", model.UsernameOrEmail);
                return BadRequest("Email/Username ou password inválidos.");
            }

            // Map user entity to DTO (excludes sensitive data)
            var userToBuildJWT = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u == user);


            if (userToBuildJWT == null)
            {
                _logger.LogWarning("Bad user to generate JWT token {user}", model.UsernameOrEmail);
                return BadRequest("Email/Username ou password inválidos.");
            }

            // CHeck if user is not bloqued
            if (!userToBuildJWT.IsActive)
            {
                _logger.LogWarning("User {UsernameOrEmail} is blocked.", model.UsernameOrEmail);
                return BadRequest("Utilizador bloqueado.");
            }

            // Update last login time
            userToBuildJWT.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(userToBuildJWT);

            var userDto = new LoggedInUserDto(
                userToBuildJWT.Person!.FirstName,
                userToBuildJWT.Person!.LastName,
                _jwtService.CreateJwt(userToBuildJWT).Result
            );
            _logger.LogInformation("User {UsernameOrEmail} logged in successfully.", model.UsernameOrEmail);
            return Ok(userDto);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("set-role")]
        public async Task<ActionResult> SetRoleToUserAsync([FromBody] UserRoleDto userRole)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            await _roleService.UpdateUserRolesAsync(userRole);

            return Ok(new OkMessage()
            {
                Title = "Papeis atribuídos com sucesso.",
                Message = "Os papéis foram atribuídos com sucesso ao usuário.",
                Data = new
                {
                    UserId = user!.Id,
                    userRole.Roles
                }
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-roles/{userId}")]
        public async Task<ActionResult<IEnumerable<string>>> GeUserRolesAsync(string userId)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            var userToModify = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("Utilizador não encontrado");

            var roles = await _userManager.GetRolesAsync(userToModify);
            return Ok(roles.ToList());
        }
    }
}
