using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Account.Services;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Account.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IAccountService _accountService;
        public AccountController(
            ILogger<AccountController> logger,
            UserManager<User> userManager,
            IAccountService accountService)
        {
            _logger = logger;
            _userManager = userManager;
            _accountService = accountService;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="model">The RegisterDto containing user registration information including:
        /// - Email: The email address of the user (must be unique)
        /// - UserName: The username of the user (must be unique)
        /// - FirstName: The user's first name
        /// - LastName: The user's last name
        /// - Password: The user's password
        /// </param>
        /// <returns>
        /// Returns IActionResult with the following possible outcomes:
        /// - BadRequest (400) if email or username already exists
        /// - BadRequest (400) if user creation fails with error details
        /// - Ok (200) with success message if registration is successful
        /// </returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");

            // Check if the user is null or if they are not an admin
            if (user == null || !await user.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            // Create the user in the identity system
            try
            {
                await _accountService.RegistUserAsync(model);

                _logger.LogInformation("User {UserName} created successfully.", model.UserName);

                return Ok(new OkMessage(
                    "Conta Criada",
                    "A sua conta foi criada, pode iniciar sessão.",
                    new { model.UserName, model.Email })
                    );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User creation failed due to existing email or username.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user registration.");
                return StatusCode(500, new { error = "Internal server error." });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("block-user/{userId}")]
        public async Task<IActionResult> BlockUserAsync(string userId)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");

            // Check if the user is null or if they are not an admin
            if (user == null || !await user.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            try
            {
                var retrieveUser = await _accountService.BlockUserAsync(userId);

                if (retrieveUser == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                    return NotFound($"Utilizador com ID {userId} não encontrado.");
                }

                return Ok(new OkMessage(
                    $"Utilizador {(retrieveUser.IsActive ? "desbloqueado" : "bloqueado")}",
                    $"O utilizador com ID {userId} foi {(retrieveUser.IsActive ? "desbloqueado" : "bloqueado")} com sucesso.",
                    new { retrieveUser })
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while blocking user with ID {UserId}.", userId);
                return BadRequest(ex);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllAsync()
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");

            // Check if the user is null or if they are not an admin
            if (user == null || !await user.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            // Get all users
            var users = await _accountService.GetAllUsersAsync();
            if (!users.Any()) // No users found
            {
                _logger.LogWarning("No users found while performing query.");
                return NotFound("Não foram encontrados utilizadores.");
            }

            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetSingleAsync(string id)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");

            // Check if the user is null or if they are not an admin
            if (user == null || !await user.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            // Get the user by ID
            var singleUser = await _accountService.GetUserByIdAsync(id);
            if (singleUser == null) // no user found
            {
                _logger.LogWarning("User not found to perform query.");
                return NotFound("Utilizador não encontrado.");
            }
            return Ok(singleUser);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("user/update/{id}")]
        public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdateUserDto model)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");

            // Check if the user is null or if they are not an admin
            if (user == null || !await user.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            // Check model id is same as id from url
            if (id != model.Id)
            {
                _logger.LogWarning("The id from the user passed on the body is not the same as the one passed on the url params");
                return BadRequest("User ID mismatch.");
            }

            // try update the user
            var result = await _accountService.UpdateUserAsync(model);
            if (!result)
            {
                _logger.LogWarning("Failed to update the user");
                return BadRequest("Falha ao editar o utilizador.");
            }

            // return message for UI
            return Ok(new OkMessage(
                "Utilizador atualizado.",
                $"Utilizador com o email {model.Email} atualizado.",
                model
                ));
        }



    }
}
