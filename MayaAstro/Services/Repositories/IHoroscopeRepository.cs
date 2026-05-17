using MayaAstro.Models;
namespace MayaAstro.Services.Repositories
{
    public interface IHoroscopeRepository
    {
        #region Rashi Type
        Task<ApiResponseModel> GetRashiList();
        Task<ApiResponseModel> AddRashi(RashiVM obj);   
        Task<ApiResponseModel> GetRashiById(int Id);
        Task<ApiResponseModel> DeleteRashi(int Id);

        #endregion  Rashi Type

        #region Horoscope Detail
        Task<ApiResponseModel> GetHoroscopeList();
        Task<ApiResponseModel> AddHoroscope(HoroscopeVM obj);
        Task<ApiResponseModel> GetHoroscopeById(int Id);
		Task<ApiResponseModel> GetHoroscopeDetailByUrl(string slug);
        Task<ApiResponseModel> DeleteHoroscope(int Id);

        Task<ApiResponseModel> TrueFalse();
        #endregion  Horoscope Detail
    }
}
