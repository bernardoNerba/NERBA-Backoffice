using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Account.Services;

public interface IAccountService
{
    Task<Result> RegistUserAsync(RegisterDto registerDto);
    Task<Result> BlockUserAsync(string userId);
    Task<Result<IEnumerable<RetrieveUserDto>>> GetAllUsersAsync();
    Task<Result<RetrieveUserDto>> GetUserByIdAsync(string id);
    Task<Result> UpdateUserAsync(UpdateUserDto model);
}
