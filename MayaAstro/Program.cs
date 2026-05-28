using AutoMapper;
using MayaAstro.DatabaseEntities;
using MayaAstro.Services.Configuration;
using MayaAstro.Services.Mappers;
using MayaAstro.Services.Repositories;
using MayaAstro.Services.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using MayaAstro.Filters;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MayaAstro.Models;
using Serilog;
using System.Security.Cryptography;
using System.Xml.Linq;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("Logs/api-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);
// Use Serilog
builder.Host.UseSerilog();
builder.Services.AddDbContext<MayaAstroContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<FirebaseApp>(services =>
{
    var firebaseConfig = builder.Configuration.GetSection("FIREBASE_CONFIG").Get<FirebaseConfig>();

    var appOptions = new AppOptions()
    {
        Credential = GoogleCredential.FromJson(firebaseConfig.ToJson())
    };

    return FirebaseApp.Create(appOptions);
});
builder.Services.AddScoped<SuperadminAuthorizationFilter>();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<YouTubeService>();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.TryAddTransient<IBlogRepository, BlogRepository>();
builder.Services.TryAddTransient<IAdminRepository, AdminRepository>();
builder.Services.TryAddTransient<IHoroscopeRepository, HoroscopeRepository>();
builder.Services.TryAddTransient<IBlogServices, BlogServices>();
builder.Services.TryAddTransient<IAdminServices, AdminServies>();
builder.Services.TryAddTransient<IHoroscopeService,HoroscopeService>();
builder.Services.TryAddTransient<ILoginRepository, LoginRepository>();
builder.Services.TryAddTransient<ILoginServices, LoginServices>();
builder.Services.TryAddTransient<IMinnieWorldRepository, MinnieWorldRepository>();
builder.Services.TryAddTransient<IMinnieWorldServices, MinnieWorldServices>();
builder.Services.TryAddSingleton<ICryptoServices, CryptoService>();
builder.Services.AddHttpClient<IJewellerServices, JewellerServices>();
builder.Services.TryAddTransient<IJewellerRepository, JewellerRepositiory>();
builder.Services.AddSingleton<FirebaseService>();
builder.Services.AddSingleton<DeviceTokenService>();
builder.Services.Configure<MayaAstro.Services.Configuration.Messages>(options => builder.Configuration.GetSection("Messages").Bind(options));
builder.Services.Configure<Token>(options => builder.Configuration.GetSection("Token").Bind(options));
builder.Services.Configure<BrevoSettings>(options => builder.Configuration.GetSection("BrevoSettings").Bind(options));
builder.Services.TryAddSingleton<IClock, Clock>();
var configuration = builder.Configuration;
builder.Services.TryAddSingleton<IMapper>(new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<Mapping>())));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var jwtSettings = builder.Configuration.GetSection("JsonWebTokenKeys");
    var signingKey = jwtSettings["IssuerSigningKey"] ?? builder.Configuration["Token:IssuerSigningKey"];

    if (string.IsNullOrWhiteSpace(signingKey))
    {
        signingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        Log.Warning("JWT signing key is missing from configuration. A temporary in-memory key was generated; configure JsonWebTokenKeys:IssuerSigningKey before production use.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = jwtSettings.GetValue("ValidateIssuer", true),
        ValidateAudience = jwtSettings.GetValue("ValidateAudience", true),
        ValidateLifetime = jwtSettings.GetValue("ValidateLifetime", true),
        ValidateIssuerSigningKey = jwtSettings.GetValue("ValidateIssuerSigningKey", true),
        ValidIssuer = jwtSettings["ValidIssuer"] ?? "https://minnieworld.com/",
        ValidAudience = jwtSettings["ValidAudience"] ?? "https://minnieworld.com/",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});
builder.Configuration.AddJsonFile("appsettings.json", true, true);
builder.Configuration.AddJsonFile("appmessages.json", true, true);
// Bind config values
var connectionString = builder.Configuration["AzureNotificationHub:ConnectionString"];
var hubName = builder.Configuration["AzureNotificationHub:HubName"];

builder.Services.AddSingleton<NotificationHubService>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<NotificationHubService>>();
    return new NotificationHubService(connectionString, hubName, logger);
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=(self)");
    context.Response.Headers.TryAdd("Content-Security-Policy", "default-src 'self' https: data: blob:; script-src 'self' 'unsafe-inline' https:; style-src 'self' 'unsafe-inline' https:; img-src 'self' https: data:; font-src 'self' https: data:; frame-ancestors 'self'; base-uri 'self'; form-action 'self'");
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();




app.MapGet("/robots.txt", (HttpContext context) =>
{
    var origin = $"{context.Request.Scheme}://{context.Request.Host}";
    var robots = $"User-agent: *\nAllow: /\nDisallow: /admin\nDisallow: /login\nSitemap: {origin}/sitemap.xml\n";
    return Results.Text(robots, "text/plain");
});

app.MapGet("/sitemap.xml", async (MayaAstroContext dbContext, HttpContext context) =>
{
    var origin = $"{context.Request.Scheme}://{context.Request.Host}";
    var staticUrls = new[]
    {
        (Path: "/", Priority: "1.0", ChangeFrequency: "daily"),
        (Path: "/blog", Priority: "0.9", ChangeFrequency: "daily"),
        (Path: "/videos", Priority: "0.8", ChangeFrequency: "weekly"),
        (Path: "/services", Priority: "0.8", ChangeFrequency: "monthly"),
        (Path: "/about", Priority: "0.6", ChangeFrequency: "monthly"),
        (Path: "/contact", Priority: "0.5", ChangeFrequency: "monthly")
    };

    var urlElements = staticUrls.Select(item => CreateSitemapUrl(origin + item.Path, DateTime.UtcNow, item.ChangeFrequency, item.Priority)).ToList();

    var contentUrls = await dbContext.BlogDetail
        .AsNoTracking()
        .Where(blog => blog.IsPublished == 1 && !blog.IsDeleted && blog.PageUrl != null && blog.PageUrl != "")
        .OrderByDescending(blog => blog.ModifiedDate)
        .Take(1000)
        .Select(blog => new { blog.PageUrl, blog.ModifiedDate, blog.TypeId })
        .ToListAsync();

    foreach (var item in contentUrls)
    {
        var slug = item.PageUrl.Trim().Replace(" ", "-").ToLowerInvariant();
        var pathPrefix = item.TypeId == 4 ? "/video/" : "/";
        urlElements.Add(CreateSitemapUrl(origin + pathPrefix + slug, item.ModifiedDate, "weekly", "0.8"));
    }

    XNamespace sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
    var document = new XDocument(new XElement(sitemapNamespace + "urlset", urlElements));
    return Results.Text(document.ToString(), "application/xml");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();


static XElement CreateSitemapUrl(string location, DateTime lastModified, string changeFrequency, string priority)
{
    XNamespace sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
    return new XElement(sitemapNamespace + "url",
        new XElement(sitemapNamespace + "loc", location),
        new XElement(sitemapNamespace + "lastmod", lastModified.ToUniversalTime().ToString("yyyy-MM-dd")),
        new XElement(sitemapNamespace + "changefreq", changeFrequency),
        new XElement(sitemapNamespace + "priority", priority));
}
