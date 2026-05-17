using MayaAstro.Services.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
namespace MayaAstro.Filters
{
    public class SuperadminAuthorizationFilter : IAuthorizationFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SuperadminAuthorizationFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!IdentityExtention.CheckSuperadminIdentity(_httpContextAccessor))
            {
                context.Result = new RedirectToActionResult("Login", "Home", null);
            }
        }
    }
}
