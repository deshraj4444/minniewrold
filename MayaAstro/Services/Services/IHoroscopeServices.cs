using MayaAstro.Models;

namespace MayaAstro.Services.Services
{
    public interface IHoroscopeService
    {
        #region Rashi Type
        Task<ApiResponseModel> GetRashiList();
        Task<ApiResponseModel> AddRashi(RashiVM obj);
        Task<ApiResponseModel> GetRashiById(int id);
        Task<ApiResponseModel> DeleteRashi(int id);
        #endregion Rashi Type

        #region Horoscope Detail
        Task<ApiResponseModel> GetHoroscopeList();
        Task<ApiResponseModel> AddHoroscope(HoroscopeVM obj);
        Task<ApiResponseModel> GetHoroscopeById(int id);
        Task<ApiResponseModel> GetHoroscopeDetailByUrl(string slug);
        Task<ApiResponseModel> DeleteHoroscope(int id);
        Task<ApiResponseModel> TrueFalse();

        #endregion Horoscope Detail
    }

}
