
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MayaAstro.Services.Extensions
{
    public static class IdentityExtention
    {
        public static bool CheckSuperadminIdentity(IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext.User.IsInRole(Constants.Constants.SuperAdmin)  || httpContextAccessor.HttpContext.User.Identity.Name != null;
        }


        public static bool CheckUserIdentity(IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext.User.IsInRole(Constants.Constants.User) || httpContextAccessor.HttpContext.User.Identity.Name != null;
        }
        public static bool CheckAstroIdentity(IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext.User.IsInRole(Constants.Constants.Astrologer);
        }
       
    }
}