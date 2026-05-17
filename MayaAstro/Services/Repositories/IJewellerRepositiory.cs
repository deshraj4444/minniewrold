using MayaAstro.DatabaseEntities;
using MayaAstro.Models;

namespace MayaAstro.Services.Repositories
{
    public interface IJewellerRepository
    {
        #region Admin - MessageBanner Management

        Task<ApiResponseModel> GetMessageBannerDetailById(int id);
        Task<ApiResponseModel> AddMessageBannerDetail(MessageBannerDetailVM model);      // INSERT

        #endregion


        #region BankDetail

        Task<ApiResponseModel> GetBankDetailList();
        Task<ApiResponseModel> DeleteBankDetail(int id);
        Task<ApiResponseModel> GetBankDetailById(int id);
        Task<ApiResponseModel> AddBankDetail(BankDetailVM model);
        #endregion
        #region CommonEnquiry
        Task<ApiResponseModel> VerifyPhone(string phone);
        Task<ApiResponseModel> AddEnquiry(CommonEnquiryVM model);
        Task<ApiResponseModel> GetEnquiryList(string type);
        Task<ApiResponseModel> GetEnquiryById(int id);
        Task<ApiResponseModel> DeleteEnquiry(int id);

        #endregion
        Task<GoldPriceData?> GetLatestGoldPriceAsync();
        Task SaveGoldPriceAsync(GoldPriceData data);

        Task<ApiResponseModel> SaveAnnouncement(AnnouncementsVM model);

        Task<ApiResponseModel> GetAnnouncementList();

        Task<ApiResponseModel> GetAnnouncementById(int id);

        Task<ApiResponseModel> GetAnnouncementByDate(DateTime? startDate, DateTime? endDate);

        Task<ApiResponseModel> DeleteAnnouncement(int id);
        Task<ApiResponseModel> SaveGoldSettings(GoldSettingsVM model);
        Task<ApiResponseModel> GetPremiumAsyncById(int Id);
        Task<ApiResponseModel> GetGoldSettingsById(int Id);


    }
}