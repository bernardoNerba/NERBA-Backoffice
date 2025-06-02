using NERBABO.ApiService.Core.Account.Models;

namespace NERBABO.ApiService.Core.Authentication.Services;

public interface IJwtService
{
    Task<string> CreateJwt(User user);
}
