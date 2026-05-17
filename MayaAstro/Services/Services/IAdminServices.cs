using MayaAstro.Models;

namespace MayaAstro.Services.Services
{
    public interface IAdminServices
    {
        #region Lecture for App


        Task<ApiResponseModel> GetLectureList();
        Task<ApiResponseModel> AddLecture(LectureCategoryVM obj);
        Task<ApiResponseModel> GetLectureById(int Id);
        Task<ApiResponseModel> DeleteLecture(int Id);

        Task<ApiResponseModel> GetLectureDetailsList();
        Task<ApiResponseModel> GetLectureListByCategory(int Id);
        Task<ApiResponseModel> AddLectureDetails(LectureDetailVM obj);
        Task<ApiResponseModel> GetLectureDetailsById(int Id);
        Task<ApiResponseModel> DeleteLectureDetails(int Id);
        #endregion
        Task<bool> SaveDeviceToken(DeviceTokenVM model);
        Task<List<string>> GetDeviceToken();
        Task<LoginVM> Authenticate(LoginVM loginViewModel);


        #region Question
        Task<ApiResponseModel> GetQuestionList();

        Task<ApiResponseModel> GetQuestionListByCategory();
        Task<ApiResponseModel> AddQuestion(QuestionVM obj);
        Task<ApiResponseModel> GetQuestionById(int Id);
        Task<ApiResponseModel> DeleteQuestion(int Id);
        Task<ApiResponseModel> GetQuestionDetailList();
        Task<ApiResponseModel> AddQuestionDetail(QuestionDetailVM obj);
        Task<ApiResponseModel> GetQuestionDetailById(int Id);
        Task<ApiResponseModel> DeleteQuestionDetail(int Id);
        Task<ApiResponseModel> GetQuickLearnList();
        Task<ApiResponseModel> AddQuickLearn(QuickLearnVM obj);
        Task<ApiResponseModel> GetQuickLearnById(int Id);
        Task<ApiResponseModel> DeleteQuickLearn(int Id);
        #endregion
        #region Contant Master
        Task<ApiResponseModel> GetContantMasterList();
        Task<ApiResponseModel> AddContantMaster(ContentMasterVM obj);
        Task<ApiResponseModel> GetContantMasterById(int Id);
        Task<ApiResponseModel> DeleteContantMaster(int Id);
        #endregion Conatnt Master
    }
}
