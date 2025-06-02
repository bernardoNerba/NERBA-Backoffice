using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Account.Services;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Account.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IAccountService _accountService;
        public AccountController(
            ILogger<AccountController> logger,
            UserManager<User> userManager,
            AppDbContext context,
            IAccountService accountService)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
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

            // Create the user in the identity system
            try
            {
                await _accountService.RegistUserAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating user {UserName}.", model.UserName);
                return BadRequest(ex);
            }

            // Display success message
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
            var user = new User();

            try
            {
                user = await _accountService.BlockUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while blocking user with ID {UserId}.", userId);
                return BadRequest(ex);
            }

            return Ok(new OkMessage(
                $"Utilizador {(user.IsActive ? "desbloqueado" : "bloqueado")}",
                $"O utilizador com ID {userId} foi {(user.IsActive ? "desbloqueado" : "bloqueado")} com sucesso.",
                new { userId })
            );

        }

    }
}
