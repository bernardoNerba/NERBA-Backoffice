using NERBABO.ApiService.Core.Account.Dtos;

namespace NERBABO.ApiService.Core.Account.Services;

public interface IAccountService
{
    Task RegistUserAsync(RegisterDto registerDto);
    Task<RetrieveUserDto?> BlockUserAsync(string userId);
    Task<IEnumerable<RetrieveUserDto>> GetAllUsersAsync();
    Task<RetrieveUserDto?> GetUserByIdAsync(string id);
    Task<bool> UpdateUserAsync(UpdateUserDto model);
    Task<bool> DeleteUserAsync(string id);
}
