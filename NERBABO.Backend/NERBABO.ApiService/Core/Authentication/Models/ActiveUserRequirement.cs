using Microsoft.AspNetCore.Authorization;

namespace NERBABO.ApiService.Core.Authentication.Models
{
    public class ActiveUserRequirement : IAuthorizationRequirement
    {
        public ActiveUserRequirement(){}
    }
}
