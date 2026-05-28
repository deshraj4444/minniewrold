using MayaAstro.Models;
using MayaAstro.Services.Repositories;

namespace MayaAstro.Services.Services
{
    public class BlogServices : IBlogServices
    {
        private readonly IBlogRepository _iBlogRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BlogServices(IBlogRepository iBlogRepository, IHttpContextAccessor httpContextAccessor)
        {
            _iBlogRepository = iBlogRepository;
            _httpContextAccessor = httpContextAccessor;
        }


        #region Blog Details

        public async Task<ApiResponseModel> BlogListImage(string? categoryName, int pageSize, int pageNo, string search, int domainId, int? typeId = null)
        {
            return await _iBlogRepository.BlogListImage(categoryName, pageSize, pageNo, search, domainId, typeId);
        }
        public async Task<ApiResponseModel> HomeBlogList(int domainId, string? categoryName)
        {
            return await _iBlogRepository.HomeBlogList(domainId, categoryName);
        }
        public async Task<ApiResponseModel> GetBlogDetailList(int typeId, int domainId)
        {
            return await _iBlogRepository.GetBlogDetailList(typeId, domainId);
        }
        public async Task<ApiResponseModel> AddBlogDetail(BlogDetailVM obj)
        {
            return await _iBlogRepository.AddBlogDetail(obj);
        }
        public async Task<ApiResponseModel> BlogExists(string name)
        {
            return await _iBlogRepository.BlogExists(name);
        }
        public async Task<ApiResponseModel> GetBlogDetailById(int Id)
        {
            return await _iBlogRepository.GetBlogDetailById(Id);
        }
        public async Task<ApiResponseModel> DeleteBlogDetail(int Id)
        {
            return await _iBlogRepository.DeleteBlogDetail(Id);
        }

        public async Task<ApiResponseModel> GetBlogDetailByUrl(string slug)
        {
            return await _iBlogRepository.GetBlogDetailByUrl(slug);
        }
        #endregion Blog Details

        #region BlogCategory

        public async Task<ApiResponseModel> GetBlogCategoryList(int typeId)
        {
            return await _iBlogRepository.GetBlogCategoryList(typeId);
        }
        public async Task<ApiResponseModel> GetBlogCategoryUserList(int domainId)
        {
            return await _iBlogRepository.GetBlogCategoryUserList(domainId);
        }
        public async Task<ApiResponseModel> AddBlogCategory(BlogCategoryVM obj)
        {
            return await _iBlogRepository.AddBlogCategory(obj);
        }
        public async Task<ApiResponseModel> GetBlogCategoryById(int Id)
        {
            return await _iBlogRepository.GetBlogCategoryById(Id);
        }
        public async Task<ApiResponseModel> DeleteBlogCategory(int Id)
        {
            return await _iBlogRepository.DeleteBlogCategory(Id);
        }

        #endregion BlogCateogry
    }
}
