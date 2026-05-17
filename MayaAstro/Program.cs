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
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "https://minnieworld.com/",
        ValidAudience = "https://minnieworld.com/",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("64A63153-11C1-4919-9133-EFAF99A9B456"))
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
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
