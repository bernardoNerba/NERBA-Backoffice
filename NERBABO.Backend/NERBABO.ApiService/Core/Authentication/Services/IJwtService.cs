using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Authentication.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Authentication.Services;

public interface IJwtService
{
    Task<Result<LoggedInUserDto>> GenerateJwtOnLoginAsync(LoginDto model);
    Task<Result<LoggedInUserDto>> GenerateRefreshTokenAsync(string userId);
}
