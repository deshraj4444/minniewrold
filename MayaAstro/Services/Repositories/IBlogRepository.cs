using MayaAstro.Models;

namespace MayaAstro.Services.Repositories
{
	public interface IBlogRepository
	{
		#region Blog Details
		Task<ApiResponseModel> BlogListImage(string? categoryName, int pageSize, int pageNo, string search, int domainId, int? typeId = null);
		Task<ApiResponseModel> HomeBlogList(int domainId , string? categoryName);
        Task<ApiResponseModel> GetBlogDetailList(int typeId, int domainId);
		Task<ApiResponseModel> AddBlogDetail(BlogDetailVM obj);
		Task<ApiResponseModel> BlogExists(string name);
		Task<ApiResponseModel> GetBlogDetailById(int Id);
		Task<ApiResponseModel> DeleteBlogDetail(int Id);
		Task<ApiResponseModel> GetBlogDetailByUrl(string slug);
		#endregion Blog Details
		#region Blog Category
		Task<ApiResponseModel> GetBlogCategoryList(int typeId);	
		Task<ApiResponseModel> GetBlogCategoryUserList(int domainId);
		Task<ApiResponseModel> AddBlogCategory(BlogCategoryVM viewmodel);
		Task<ApiResponseModel> GetBlogCategoryById(int Id);
		Task<ApiResponseModel> DeleteBlogCategory(int Id);
		#endregion Blog Category
	}
}
