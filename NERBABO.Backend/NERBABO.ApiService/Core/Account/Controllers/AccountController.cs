using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Account.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Account.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(
        UserManager<User> userManager,
        IAccountService accountService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IAccountService _accountService = accountService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAccountsAsync()
        {
            Result<IEnumerable<RetrieveUserDto>> result = await _accountService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("user/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSingleAsync(string id)
        {
            Result<RetrieveUserDto> result = await _accountService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        [HttpPut("user/update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdateUserDto model)
        {
            if (id != model.Id)
                return BadRequest("ID mismatch.");

            Result<RetrieveUserDto> result = await _accountService.UpdateAsync(model);
            return _responseHandler.HandleResult(result);
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
        /// - PersonId: The person id associated with the user
        /// </param>
        /// <returns>
        /// Returns IActionResult with the following possible outcomes:
        /// - BadRequest (400) if email or username already exists
        /// - BadRequest (400) if user creation fails with error details
        /// - BadRequest (400) if role assignment fails with error details
        /// - BadRequest (404) if person associated with the user is not found
        /// - Created (201) if registration is successful
        /// </returns>
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAccountAsync(RegisterDto model)
        {
            Result<RetrieveUserDto> result = await _accountService.CreateAsync(model);
            return _responseHandler.HandleResult(result);
        }

        [HttpPut("block-user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockAccountAsync(string userId)
        {
            Result result = await _accountService.BlockAsync(userId);
            return _responseHandler.HandleResult(result);
        }


    }
}
