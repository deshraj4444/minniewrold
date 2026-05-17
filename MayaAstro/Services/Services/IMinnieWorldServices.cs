using MayaAstro.Models;

namespace MayaAstro.Services.Services
{
    public interface IMinnieWorldServices
    {
        #region Contacts

        Task<ApiResponseModel> GetContactList();
        Task<ApiResponseModel> AddContact(ContactsVM obj);
        Task<ApiResponseModel> GetContactById(int Id);
        Task<ApiResponseModel> DeleteContact(int Id);

        #endregion Contacts
    }
}
