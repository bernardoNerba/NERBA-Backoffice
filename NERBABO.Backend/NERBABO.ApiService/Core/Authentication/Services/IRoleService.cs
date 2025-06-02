
using NERBABO.ApiService.Core.Authentication.Dtos;

namespace NERBABO.ApiService.Core.Authentication.Services;

public interface IRoleService
{
    Task UpdateUserRolesAsync(UserRoleDto userRole);

}
