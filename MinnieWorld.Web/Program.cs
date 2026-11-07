using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MinnieWorld.Web.Data;
using MinnieWorld.Web.Endpoints;
using MinnieWorld.Web.Infrastructure;
using MinnieWorld.Web.Models;
using MinnieWorld.Web.Services;
using MinnieWorld.Web.Services.Astrology;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MinnieWorldDb")));

builder.Services.AddHttpClient<HoroscopeService>(client => client.Timeout = TimeSpan.FromSeconds(8));
builder.Services.AddHttpClient<PlaceService>(client => client.Timeout = TimeSpan.FromSeconds(6));
builder.Services.AddSingleton<MinnieWorld.Web.Services.Cbse.CbseCatalog>();
builder.Services.AddSingleton<SpaHost>();
builder.Services.AddSingleton<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();
builder.Services.AddSingleton<IPasswordHasher<Student>, PasswordHasher<Student>>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "mw_admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        // This is an API: answer 401/403 instead of redirecting to a login page.
        options.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
    })
    // Students (GK mock tests) sign in with their own long-lived cookie, so a student login never grants admin access.
    .AddCookie(ExamEndpoints.StudentScheme, options =>
    {
        options.Cookie.Name = "mw_student";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = ExamEndpoints.ValidateStudentAsync;
        options.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
    });
builder.Services.AddAuthorization(options =>
    options.AddPolicy(ExamEndpoints.StudentPolicy, p => p.AddAuthenticationSchemes(ExamEndpoints.StudentScheme).RequireAuthenticatedUser()));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(RateLimits.Forms, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
    options.AddPolicy(RateLimits.Login, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(5) }));
    // The visitor counter: generous for shared IPs (schools, mobile networks) but stops a script inflating it.
    options.AddPolicy(RateLimits.Visits, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(10) }));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    // Most hosts (IIS, nginx, Azure, Cloudflare) terminate HTTPS in front of the app.
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.XContentTypeOptions = "nosniff";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers.XFrameOptions = "SAMEORIGIN";
    await next();
});

// 301s for moved pages (e.g. /gk/jee → /jee), before anything else answers the request.
app.UseSeoRedirects();

// Built React assets (hashed file names, so they can be cached for a long time).
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.Context.Request.Path.StartsWithSegments("/assets"))
            ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
    }
});

var uploadsRoot = AdminEndpoints.UploadsRoot(app.Environment);
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads"
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapPublicEndpoints();
app.MapAuthEndpoints();
app.MapAdminEndpoints();
app.MapExamEndpoints();
app.MapSeoEndpoints();
app.MapCbseEndpoints();

// Unknown /api routes are real 404s; everything else is a client-side route served by the React app.
app.Map("/api/{**rest}", () => Results.NotFound());
app.MapFallback((HttpContext ctx, SpaHost spa, AppDbContext db) => spa.ServeAsync(ctx, db));

await DbSeeder.MigrateAndSeedAsync(app.Services);

app.Run();
