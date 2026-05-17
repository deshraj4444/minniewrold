using MayaAstro.Models;

namespace MayaAstro.Services.Repositories
{
    public interface IMinnieWorldRepository
    {
        #region Contacts

        Task<ApiResponseModel> GetContactList();
        Task<ApiResponseModel> AddContact(ContactsVM obj);
        Task<ApiResponseModel> GetContactById(int Id);
        Task<ApiResponseModel> DeleteContact(int Id);

        #endregion Contacts
    }
}
