using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Core.Account.Models;

namespace NERBABO.ApiService.Core.Account.Services;

public interface IAccountService
{
    Task RegistUserAsync(RegisterDto registerDto);
    Task<User> BlockUserAsync(string userId);
}
