
using NERBABO.ApiService.Core.Authentication.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Authentication.Services;

public interface IRoleService
{
    Task<Result> UpdateUserRolesAsync(UserRoleDto userRole);

}
