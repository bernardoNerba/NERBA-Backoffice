using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NERBABO.ApiService.Core.Account.Models;
using System.Security.Claims;

namespace NERBABO.ApiService.Core.Authentication.Models
{
    public class ActiveUserHandler(
        UserManager<User> userManager
        ) : AuthorizationHandler<ActiveUserRequirement>
    {
        private readonly UserManager<User> _userManager = userManager;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ActiveUserRequirement requirement)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Fail();
                return;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null || !user.IsActive)
            {
                context.Fail();
                return;
            }
                
            context.Succeed(requirement);
        }
    }
}
