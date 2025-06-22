using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Account.Services;

public interface IAccountService
    : IGenericService<RetrieveUserDto, RegisterDto, UpdateUserDto, string>
{
    /// <summary>
    /// Toggles the active status of a user (blocks/unblocks).
    /// Does not allow to block users with admin role.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user to block/unblock.
    /// </param>
    /// <returns>
    /// A Task representing the asynchronous operation.
    /// </returns>
    Task<Result> BlockAsync(string id);
}
