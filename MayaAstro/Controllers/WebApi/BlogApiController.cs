using MayaAstro.Models;
using MayaAstro.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MayaAstro.Controllers.WebApi
{
    [ApiController]
    [Authorize]
    public class BlogApiController : ControllerBase
    {
        private readonly IBlogServices _iBlogServices;

        public BlogApiController(IBlogServices iBlogServices)
        {
            _iBlogServices = iBlogServices;
        }

        #region Add Blog Details
      
        [HttpGet]
        [Route("api/GetBlogDetailList/{typeId}/{domainId}")]
        public async Task<IActionResult> GetBlogDetailList(int typeId,int domainId)
        {
            var data = await _iBlogServices.GetBlogDetailList(typeId, domainId);
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("api/BlogListImage/{pageSize}/{pageNo}/{domainId}/{typeId?}")]

        public async Task<IActionResult> BlogListImage(string? categoryName, int pageSize, int pageNo, string? search, int domainId, int? typeId = null)
        {
            var data = await _iBlogServices.BlogListImage(categoryName, pageSize, pageNo, search, domainId, typeId);

            return Ok(data);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("api/HomeBlogList/{domainId}")]

        public async Task<IActionResult> HomeBlogList(int domainId ,string? categoryName)
        {
            var data = await _iBlogServices.HomeBlogList(domainId, categoryName );

            return Ok(data);
        }
        [HttpPost]
        [Route("api/AddBlogDetail/")]
        public async Task<IActionResult> AddBlogDetail(BlogDetailVM obj)
        {
            var data = await _iBlogServices.AddBlogDetail(obj);
            return Ok(data);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("api/BlogExists/{title}")]
        public async Task<IActionResult> BlogExists(string title)
        {
            var data = await _iBlogServices.BlogExists(title);
            return Ok(data);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("api/GetBlogDetailByUrl/{slug}")]
        public async Task<IActionResult> GetBlogDetailByUrl(string slug)
        {
            var data = await _iBlogServices.GetBlogDetailByUrl(slug);
            return Ok(data);

        }
        [HttpGet]
        [AllowAnonymous]
        [Route("api/GetBlogDetailById/{Id}")]
        public async Task<IActionResult> GetBlogDetailById(int Id)
        {
            var data = await _iBlogServices.GetBlogDetailById(Id);
            return Ok(data);

        }
        [HttpGet]
        [Route("api/DeleteBlogDetail/{Id}")]
        public async Task<IActionResult> DeleteBlogDetail(int Id)
        {
            var data = await _iBlogServices.DeleteBlogDetail(Id);
            return Ok(data);
        }
        #endregion Add Blog Details

        #region Blog Category

        [HttpGet]
        [AllowAnonymous]
        [Route("api/GetBlogCategoryList/{typeId}")]

        public async Task<IActionResult> GetBlogCategoryList(int typeId)
        {
            var data = await _iBlogServices.GetBlogCategoryList(typeId);
            return Ok(data);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("api/GetBlogCategoryUserList/{domainId}")]
        public async Task<IActionResult> GetBlogCategoryUserList(int domainId)
        {
            var data = await _iBlogServices.GetBlogCategoryUserList( domainId);
            return Ok(data);
        }
        [HttpPost]
        [Route("api/AddBlogCategory/")]
        public async Task<IActionResult> AddBlogCategory(BlogCategoryVM obj)
        {
            var data = await _iBlogServices.AddBlogCategory(obj);
            return Ok(data);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/GetBlogCategoryById/{Id}")]
        public async Task<IActionResult> GetBlogCategoryById(int Id)
        {
            var data = await _iBlogServices.GetBlogCategoryById(Id);
            return Ok(data);
        }
        [HttpGet]
        [Route("api/DeleteBlogCategory/{Id}")]
        public async Task<IActionResult> DeleteBlogCategory(int Id)
        {
            var data = await _iBlogServices.DeleteBlogCategory(Id);
            return Ok(data);
        }
        #endregion Blog Details
    }
}
