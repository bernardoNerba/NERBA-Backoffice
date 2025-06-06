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
                (ClaimTypes.NameIdentifier)?.Value 
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            // Create the user in the identity system
            await _accountService.RegistUserAsync(model);

            _logger.LogInformation("User {UserName} created successfully.", model.UserName);

            return Ok(new OkMessage(
                "Conta Criada",
                "A sua conta foi criada, pode iniciar sessão.",
                new { model.UserName, model.Email })
                );
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("block-user/{userId}")]
        public async Task<IActionResult> BlockUserAsync(string userId)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            // block user
            await _accountService.BlockUserAsync(userId);

            return Ok(new OkMessage(
                "Estado da conta do utilizador alterado.",
                "Estado da conta do utilizador alterado com sucesso.",
                null
            ));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllAsync()
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

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
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            // Get the user by ID
            var singleUser = await _accountService.GetUserByIdAsync(id)
                ?? throw new KeyNotFoundException("Utilizador não encontrado.");
            
            return Ok(singleUser);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("user/update/{id}")]
        public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdateUserDto model)
        {
            // Check model id is same as id from url
            if (id != model.Id)
            {
                return BadRequest("User ID mismatch.");
            }

            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            // try update the user
            await _accountService.UpdateUserAsync(model);

            // return message for UI
            return Ok(new OkMessage(
                "Utilizador atualizado.",
                $"Utilizador com o email {model.Email} atualizado.",
                model
                ));
        }



    }
}
