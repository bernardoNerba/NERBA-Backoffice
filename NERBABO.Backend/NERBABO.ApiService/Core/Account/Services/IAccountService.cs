using NERBABO.ApiService.Core.Account.Dtos;

namespace NERBABO.ApiService.Core.Account.Services;

public interface IAccountService
{
    Task RegistUserAsync(RegisterDto registerDto);
    Task BlockUserAsync(string userId);
    Task<IEnumerable<RetrieveUserDto>> GetAllUsersAsync();
    Task<RetrieveUserDto> GetUserByIdAsync(string id);
    Task UpdateUserAsync(UpdateUserDto model);
}
