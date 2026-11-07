using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MinnieWorld.Web.Contracts;
using MinnieWorld.Web.Data;
using MinnieWorld.Web.Infrastructure;
using MinnieWorld.Web.Models;

namespace MinnieWorld.Web.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth").AddEndpointFilter<RequireCsrfHeaderFilter>();

        auth.MapPost("/login", async (LoginRequest req, AppDbContext db, IPasswordHasher<AdminUser> hasher, HttpContext http) =>
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Email == email);
            var result = user is null ? PasswordVerificationResult.Failed : hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
            if (user is null || result == PasswordVerificationResult.Failed)
            {
                return Results.Problem("Incorrect email or password.", statusCode: StatusCodes.Status401Unauthorized);
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = hasher.HashPassword(user, req.Password);
            }

            user.LastLoginAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.DisplayName)
            ], CookieAuthenticationDefaults.AuthenticationScheme);

            await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return Results.Ok(new { user.Email, user.DisplayName });
        }).Validate<LoginRequest>().RequireRateLimiting(RateLimits.Login);

        auth.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.NoContent();
        });

        auth.MapGet("/me", (ClaimsPrincipal user) =>
            user.Identity?.IsAuthenticated == true
                ? Results.Ok(new { Email = user.FindFirstValue(ClaimTypes.Email), DisplayName = user.Identity.Name })
                : Results.Unauthorized());
    }
}
