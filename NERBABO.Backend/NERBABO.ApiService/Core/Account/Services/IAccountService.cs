using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Account.Services;

public interface IAccountService
    : IGenericService<RetrieveUserDto, RegisterDto, UpdateUserDto, string>
{
    Task<Result> BlockAsync(string id);
}
