using MayaAstro.Models;
using MayaAstro.Services.Repositories;

namespace MayaAstro.Services.Services
{
    public class MinnieWorldServices : IMinnieWorldServices
    {
        private readonly IMinnieWorldRepository _iminnieWorldRepository;
        private readonly ICryptoServices _cryptoServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MinnieWorldServices(IMinnieWorldRepository iminnieWorldRepository, ICryptoServices cryptoServices, IHttpContextAccessor httpContextAccessor)
        {
            _iminnieWorldRepository = iminnieWorldRepository;
            _httpContextAccessor = httpContextAccessor;
            _cryptoServices = cryptoServices;
        }
        #region Contacts
        public async Task<ApiResponseModel> GetContactList()
        {
            return await _iminnieWorldRepository.GetContactList();
        }
        public async Task<ApiResponseModel> AddContact(ContactsVM obj)
        {
            return await _iminnieWorldRepository.AddContact(obj);
        }
        public async Task<ApiResponseModel> GetContactById(int Id)
        {
            return await _iminnieWorldRepository.GetContactById(Id);
        }
        public async Task<ApiResponseModel> DeleteContact(int Id)
        {
            return await _iminnieWorldRepository.DeleteContact(Id);
        }

        #endregion Contacts
    }
}
