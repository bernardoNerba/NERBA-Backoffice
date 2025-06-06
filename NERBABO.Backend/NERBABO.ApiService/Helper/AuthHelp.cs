using Microsoft.AspNetCore.Identity;
using NERBABO.ApiService.Core.Account.Models;

namespace NERBABO.ApiService.Helper
{
    public class AuthHelp
    {
        public async static Task CheckUserHasRoleAndActive(User user, string role, UserManager<User> userManager)
        {
            if ((user == null) || !await userManager.IsInRoleAsync(user, role) || !user.IsActive)
                throw new UnauthorizedAccessException("Não está autorizado a aceder a esta informação.");
        }

        internal static async Task CheckUserHasRoleAndActive(object value, string v, UserManager<User> userManager)
        {
            throw new NotImplementedException();
        }
    }
}
