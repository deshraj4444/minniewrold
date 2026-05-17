
using Microsoft.AspNetCore.Authorization;
using MayaAstro.Models;
using MayaAstro.Services.Repositories;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using AutoMapper;
using MayaAstro.Services.Extensions;
namespace MayaAstro.Services.Services

{
    public class LoginServices : ILoginServices
    {
        public readonly ILoginRepository _iLoginRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly ICryptoServices _cryptoService;
        public IConfiguration _config { get; set; }
        public LoginServices(IConfiguration config, ILoginRepository iLoginRepository, IHttpContextAccessor httpContextAccessor, ICryptoServices cryptoService)
        {
            _iLoginRepository = iLoginRepository;
            _httpContextAccessor = httpContextAccessor;
            _cryptoService = cryptoService;
            _config = config;

        }
        #region Users
        public async Task<ApiResponseModel> RegisterUser(UserVM userProfileModel)
        {
            if (userProfileModel.Password != null)
            {
                if (userProfileModel.Id == ("00000000-0000-0000-0000-000000000000").ConvertToGuid())
                {
                    var saltAndHash = await GetPasswordHash(userProfileModel.Password);
                    var newPassword = saltAndHash.Split(":");
                    if (newPassword.Length != 2)
                    {
                        return null;
                    }

                    userProfileModel.Salt = newPassword[0];
                    userProfileModel.Password = newPassword[1];
                }
            }

            return await _iLoginRepository.RegisterUser(userProfileModel);
        }
        private async Task<string> GetPasswordHash(string newPassword)
        {
            return await _cryptoService.HashPassword(newPassword);
        }
        public async Task<ApiResponseModel> GetUserList()
        {
            return await _iLoginRepository.GetUserList();

        }

        public async Task<ApiResponseModel> GetUserById(Guid id)
        {
            return await _iLoginRepository.GetUserById(id);

        }
        public async Task<ApiResponseModel> DeleteUser(Guid id)
        {
            return await _iLoginRepository.DeleteUser(id);

        }

        public async Task<ApiResponseModel> EmailExists(string email)
        {
            return await _iLoginRepository.EmailExists(email);
        }

        #endregion Users

        #region Login
        [AllowAnonymous]
        public async Task<LoginVM> Authenticate(LoginVM loginmodel)
        {
            // Static user check
            if (loginmodel.Email.ToLower() == "jjpadmin@gmail.com" && loginmodel.Password == "jjpadmin")
            {
                return new LoginVM
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), // fixed GUID
                    Email = loginmodel.Email,
                    FirstName = "Admin",
                    LastName = "User",
                    Password = "jjpadmin",
                    AuthToken = generateJwtToken(new LoginVM { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Email = loginmodel.Email })
                };
            }

            // Normal login via DB
            LoginVM loginViewModel = await _iLoginRepository.GetAdminUser(loginmodel);
            if (loginViewModel == null)
                return null;

            if (!await _cryptoService.ValidatePassword(loginmodel.Password, loginViewModel.Salt, loginViewModel.Password))
                return null;

            loginViewModel.AuthToken = generateJwtToken(loginViewModel);
            return loginViewModel;
        }

        public async Task<ApiResponseModel> Logout()
        {
            var response = new ApiResponseModel();
            try
            {
                // Get User ID from Claims
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Sign Out from Cookie Authentication
                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Clear JWT Token by Removing Authentication Header
                _httpContextAccessor.HttpContext.Response.Headers.Remove("Authorization");

                response.Status = true;
                response.Message = "Logout successful";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Logout failed: " + ex.Message;
            }
            return response;
        }
        public async Task<ApiResponseModel> GetUserCurrentWebsiteId(Guid userId)
        {
            return await _iLoginRepository.GetUserCurrentWebsiteId(userId);
        }


        public async Task<ApiResponseModel> UpdateUserWebsite(UserWebsiteVM model)
        {
            return await _iLoginRepository.UpdateUserWebsite(model);
        }


        private string generateJwtToken(LoginVM user)
        {
            try
            {

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JsonWebTokenKeys:IssuerSigningKey"]));
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion Login

        #region Websites
        public async Task<ApiResponseModel> GetWebsiteList()
        {
            return await _iLoginRepository.GetWebsiteList();
        }
        #endregion Websites
    }
}
