using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Core.Account.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Account.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(
        IAccountService accountService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly IAccountService _accountService = accountService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Gets all the accounts in the system.
        /// </summary>
        /// <response code="200">There are accounts registered in the system. Returns a List of RetrieveUserDto's.</response>
        /// <response code="404">There are no accounts registered in the system.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not admin or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAccountsAsync()
        {
            Result<IEnumerable<RetrieveUserDto>> result = await _accountService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets the account with requested id.
        /// </summary>
        /// <param name="id">The id of the user to be retrieved.</param>
        /// <response code="200">The user with the given id exists. Returns the RetrieveUserDto object.</response>
        /// <response code="404">The user with the give id does not exists.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not admin or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("user/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSingleAsync(string id)
        {
            Result<RetrieveUserDto> result = await _accountService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Updates the user with the given id.
        /// </summary>
        /// <param name="id">The id of the user to be updated.</param>
        /// <param name="model">The UpdateUserDto object that will be validated and used to update the user.</param>
        /// <response code="200">Updated Successfully. Returns the RetrieveUserDto updated object.</response>
        /// <response code="400">Validation ERROR when validating newPassword field.</response>
        /// <response code="404">The user with the give id does not exists.</response>
        /// <response code="401">The user is not authorized, invalid jwt,
        /// user is not admin or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
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
        /// Registers a new account for a person, becoming a user.
        /// </summary>
        /// <param name="model">The RegisterDto object that will be validated and used to create the account.</param>
        /// <response code="201">Created Successfully. Returns the RetrieveUserDto created object.</response>
        /// <response code="400">Validation ERROR when validating email, username, password format, person already has account, assigning roles.</response>
        /// <response code="404">The person with the give personId does not exists.</response>
        /// <response code="401">The user is not authorized, invalid jwt,
        /// user is not admin or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAccountAsync(RegisterDto model)
        {
            Result<RetrieveUserDto> result = await _accountService.CreateAsync(model);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Blocks the account of a given user by userId.
        /// </summary>
        /// <param name="userId">The id of the user to be blocked.</param>
        /// <response code="200">Blocked Successfully.</response>
        /// <response code="400">Validation ERROR is not possible to block a user with the role of Admin or updating the user instance error.</response>
        /// <response code="404">The user with the given id does not exists.</response>
        /// <response code="401">The user is not authorized, invalid jwt,
        /// user is not admin or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("block-user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockAccountAsync(string userId)
        {
            Result result = await _accountService.BlockAsync(userId);
            return _responseHandler.HandleResult(result);
        }
    }
}
