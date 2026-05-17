using AutoMapper;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;

namespace MayaAstro.Services.Repositories
{
    public interface ILoginRepository
    {
        #region Users

        Task<ApiResponseModel> RegisterUser(UserVM userProfileModel);
        Task<ApiResponseModel> GetUserList();
        Task<ApiResponseModel> GetUserById(Guid id);
        Task<ApiResponseModel> DeleteUser(Guid id);
        Task<ApiResponseModel> EmailExists(string email);

        #endregion Users

        #region Login Api
        Task<Users> UpdateAuthToken(LoginVM userprofilemodel);
        Task<LoginVM> GetAdminUser(LoginVM loginViewModel);
        Task<ApiResponseModel> GetUserCurrentWebsiteId(Guid userId);

        Task<ApiResponseModel> UpdateUserWebsite(UserWebsiteVM model);

        #endregion Login Api

        #region Websites
        Task<ApiResponseModel> GetWebsiteList();

        
        #endregion Websites
    }
}
