using MayaAstro.Models;
using MayaAstro.Services.Repositories;

namespace MayaAstro.Services.Services
{
    public class HoroscopeService : IHoroscopeService
    {
        private readonly IHoroscopeRepository _iHoroscopeRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HoroscopeService(IHoroscopeRepository iHoroscopeRepository, IHttpContextAccessor httpContextAccessor)
        {
            _iHoroscopeRepository = iHoroscopeRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Rashi Type

        public async Task<ApiResponseModel> GetRashiList()
        {
            return await _iHoroscopeRepository.GetRashiList();
        }

        public async Task<ApiResponseModel> AddRashi(RashiVM obj)
        {
            return await _iHoroscopeRepository.AddRashi(obj);
        }

        public async Task<ApiResponseModel> GetRashiById(int id)
        {
            return await _iHoroscopeRepository.GetRashiById(id);
        }

        public async Task<ApiResponseModel> DeleteRashi(int id)
        {
            return await _iHoroscopeRepository.DeleteRashi(id);
        }

        #endregion Rashi Type

        #region Horoscope Detail

        public async Task<ApiResponseModel> GetHoroscopeList()
        {
            return await _iHoroscopeRepository.GetHoroscopeList();
        }
		public async Task<ApiResponseModel> GetHoroscopeDetailByUrl(string slug)
		{
			return await _iHoroscopeRepository.GetHoroscopeDetailByUrl(slug);
		}
		public async Task<ApiResponseModel> AddHoroscope(HoroscopeVM obj)
        {
            return await _iHoroscopeRepository.AddHoroscope(obj);
        }

        public async Task<ApiResponseModel> GetHoroscopeById(int id)
        {
            return await _iHoroscopeRepository.GetHoroscopeById(id);
        }

        public async Task<ApiResponseModel> DeleteHoroscope(int id)
        {
            return await _iHoroscopeRepository.DeleteHoroscope(id);
        }

        public async Task<ApiResponseModel> TrueFalse()
        {
            return await _iHoroscopeRepository.TrueFalse();
        }

        #endregion Horoscope Detail
    }
}
