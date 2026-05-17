using AutoMapper;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;
namespace MayaAstro.Services.Services

{
    public interface ILoginServices
    {
        #region Users
        Task<ApiResponseModel> RegisterUser(UserVM userProfileModel);
        Task<ApiResponseModel> GetUserList();
        Task<ApiResponseModel> GetUserById(Guid id);
        Task<ApiResponseModel> DeleteUser(Guid id);
        Task<ApiResponseModel> EmailExists(string email);
        #endregion User
        #region Login Api
        Task<LoginVM> Authenticate(LoginVM loginViewModel);
        Task<ApiResponseModel> Logout();
        Task<ApiResponseModel> GetUserCurrentWebsiteId(Guid userId);

        Task<ApiResponseModel> UpdateUserWebsite(UserWebsiteVM model);
        #endregion Login Api
        #region Websites
        Task<ApiResponseModel> GetWebsiteList();
        #endregion Websites
    }
}
