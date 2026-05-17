using MayaAstro.Models;

namespace MayaAstro.Services.Services
{
    public interface IBlogServices 
    {

        #region Blog Details
        Task<ApiResponseModel> BlogListImage(string? categoryName, int pageSize, int pageNo, string search,int domainId);
        Task<ApiResponseModel> HomeBlogList(int domainId, string? categoryName);

        Task<ApiResponseModel> GetBlogDetailList(int typeId, int domainId);
        Task<ApiResponseModel> AddBlogDetail(BlogDetailVM obj);
        Task<ApiResponseModel> BlogExists(string name);
        Task<ApiResponseModel> GetBlogDetailById(int Id);
        Task<ApiResponseModel> DeleteBlogDetail(int Id);
		Task<ApiResponseModel> GetBlogDetailByUrl(string slug);

		#endregion Blog Details

		#region BlogCategory

		Task<ApiResponseModel> GetBlogCategoryList(int typeId);
        Task<ApiResponseModel> GetBlogCategoryUserList(int domainId);
        Task<ApiResponseModel> AddBlogCategory(BlogCategoryVM obj);
        Task<ApiResponseModel> GetBlogCategoryById(int Id);
        Task<ApiResponseModel> DeleteBlogCategory(int Id);

        #endregion BlogCategory


    }
}
