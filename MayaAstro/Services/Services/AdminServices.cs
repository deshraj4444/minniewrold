using MayaAstro.Models;
using MayaAstro.Services.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MayaAstro.Services.Services
{
    public class AdminServies : IAdminServices
    {
        private readonly IAdminRepository _iAdminRepository;
        private readonly ICryptoServices _cryptoServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AdminServies(
            IAdminRepository iAdminRepository,
            ICryptoServices cryptoServices,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _iAdminRepository = iAdminRepository;
            _httpContextAccessor = httpContextAccessor;
            _cryptoServices = cryptoServices;
            _configuration = configuration;
        }

        #region Lecture For App
        public async Task<ApiResponseModel> GetLectureList()
        {
            return await _iAdminRepository.GetLectureList();
        }

        public async Task<ApiResponseModel> AddLecture(LectureCategoryVM obj)
        {
            return await _iAdminRepository.AddLecture(obj);
        }
        public async Task<ApiResponseModel> GetLectureById(int Id)
        {
            return await _iAdminRepository.GetLectureById(Id);
        }
        public async Task<ApiResponseModel> DeleteLecture(int Id)
        {
            return await _iAdminRepository.DeleteLecture(Id);
        }


        public async Task<ApiResponseModel> GetLectureDetailsList()
        {
            return await _iAdminRepository.GetLectureDetailsList();
        }
        public async Task<ApiResponseModel> GetLectureListByCategory(int Id)
        {
            return await _iAdminRepository.GetLectureListByCategory(Id);
        }
        public async Task<ApiResponseModel> AddLectureDetails(LectureDetailVM obj)
        {
            return await _iAdminRepository.AddLectureDetails(obj);
        }

        public async Task<ApiResponseModel> GetLectureDetailsById(int Id)
        {
            return await _iAdminRepository.GetLectureDetailsById(Id);
        }
        public async Task<ApiResponseModel> DeleteLectureDetails(int Id)
        {
            return await _iAdminRepository.DeleteLectureDetails(Id);
        }







        #endregion



        Task<bool> IAdminServices.SaveDeviceToken(DeviceTokenVM model)
        {
            return _iAdminRepository.SaveDeviceToken(model);
        }

        Task<List<string>> IAdminServices.GetDeviceToken()
        {
            return _iAdminRepository.GetDeviceToken();
        }
        #region Login
        [AllowAnonymous]
        public async Task<LoginVM> Authenticate(LoginVM loginmodel)
        {
            LoginVM loginViewModel;

            // 1️⃣ Check for static admin login
            if (loginmodel.Email.ToLower().Trim() == "jjpadmin@gmail.com" &&
                loginmodel.Password == "jjpadmin")
            {
                loginViewModel = new LoginVM
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), // fixed GUID
                    Email = "jjpadmin@gmail.com",
                    FirstName = "Admin",
                    LastName = "User",
                    Password = "jjpadmin"
                };
            }
            else
            {
                // 2️⃣ Normal DB login
                loginViewModel = await _iAdminRepository.GetAdminUser(loginmodel);

                if (loginViewModel == null)
                {
                    return null;
                }

                // Validate hashed password
                if (!await _cryptoServices.ValidatePassword(loginmodel.Password, loginViewModel.Salt, loginViewModel.Password))
                {
                    return null;
                }
            }

            // ✅ Generate JWT token
            loginViewModel.AuthToken = generateJwtToken(loginViewModel);

            // Update token in DB only for real users
            if (loginmodel.Email.ToLower().Trim() != "jjpadmin@gmail.com")
            {
                await _iAdminRepository.UpdateAuthToken(loginViewModel);
            }

            // Clear sensitive info
            loginViewModel.Password = null;
            loginViewModel.Salt = null;

            return loginViewModel;
        }
        private string generateJwtToken(LoginVM user)
        {
            // generate token that is valid for 1 days
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtKey = _configuration["JsonWebTokenKeys:IssuerSigningKey"]
                    ?? _configuration["Token:SecurityKey"]
                    ?? throw new InvalidOperationException("JWT signing key is not configured.");
                var key = Encoding.ASCII.GetBytes(jwtKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
        #endregion Login

        #region Question
        public async Task<ApiResponseModel> GetQuestionList()
        {
            return await _iAdminRepository.GetQuestionList();
        }
        public async Task<ApiResponseModel> GetQuestionListByCategory()
        {
            return await _iAdminRepository.GetQuestionListByCategory();
        }
        public async Task<ApiResponseModel> AddQuestion(QuestionVM obj)
        {
            return await _iAdminRepository.AddQuestion(obj);
        }

        public async Task<ApiResponseModel> GetQuestionById(int Id)
        {
            return await _iAdminRepository.GetQuestionById(Id);
        }
        public async Task<ApiResponseModel> DeleteQuestion(int Id)
        {
            return await _iAdminRepository.DeleteQuestion(Id);
        }
        public async Task<ApiResponseModel> GetQuestionDetailList()
        {
            return await _iAdminRepository.GetQuestionDetailList();
        }

        public async Task<ApiResponseModel> AddQuestionDetail(QuestionDetailVM obj)
        {
            return await _iAdminRepository.AddQuestionDetail(obj);
        }

        public async Task<ApiResponseModel> GetQuestionDetailById(int Id)
        {
            return await _iAdminRepository.GetQuestionDetailById(Id);
        }
        public async Task<ApiResponseModel> DeleteQuestionDetail(int Id)
        {
            return await _iAdminRepository.DeleteQuestionDetail(Id);
        }
        public async Task<ApiResponseModel> GetQuickLearnList()
        {
            return await _iAdminRepository.GetQuickLearnList();
        }

        public async Task<ApiResponseModel> AddQuickLearn(QuickLearnVM obj)
        {
            return await _iAdminRepository.AddQuickLearn(obj);
        }

        public async Task<ApiResponseModel> GetQuickLearnById(int Id)
        {
            return await _iAdminRepository.GetQuickLearnById(Id);
        }
        public async Task<ApiResponseModel> DeleteQuickLearn(int Id)
        {
            return await _iAdminRepository.DeleteQuickLearn(Id);
        }
        #endregion
        #region ConatntMaster
        public async Task<ApiResponseModel> GetContantMasterList()
        {
            return await _iAdminRepository.GetContantMasterList();
        }

        public async Task<ApiResponseModel> AddContantMaster(ContentMasterVM obj)
        {
            return await _iAdminRepository.AddContantMaster(obj);
        }

        public async Task<ApiResponseModel> GetContantMasterById(int Id)
        {
            return await _iAdminRepository.GetContantMasterById(Id);
        }
        public async Task<ApiResponseModel> DeleteContantMaster(int Id)
        {
            return await _iAdminRepository.DeleteContantMaster(Id);
        }
        #endregion ContantMaster
    }
}
