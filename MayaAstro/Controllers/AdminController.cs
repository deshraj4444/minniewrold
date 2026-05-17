using MayaAstro.Models;
using MayaAstro.Services.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using MayaAstro.DatabaseEntities;
using ImageMagick;
using MayaAstro.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using MayaAstro.Services.Extensions;
using System.Diagnostics;
namespace MayaAstro.Controllers
{
    [ServiceFilter(typeof(SuperadminAuthorizationFilter))]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [AllowAnonymous]
    public class AdminController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<Messages> _messages;
        private readonly HttpClient _client;
        HttpResponseMessage responseMessage = new HttpResponseMessage();
        private readonly IConfiguration _configuration;
        private readonly MayaAstroContext _Context;
        public IConfiguration _config { get; set; }
        public AdminController(ILogger<HomeController> logger ,IConfiguration config, IHttpContextAccessor httpContextAccessor, IOptions<Messages> messages, IConfiguration configuration, MayaAstroContext context)
        {
            _logger = logger;
            _client = new HttpClient();
            var apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
            _client.BaseAddress = new Uri(apiBaseUrl);
            _messages = messages;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _Context = context;
            _config = config;
        }
        [Route("dashboard")]
        public async Task<IActionResult> Index()
        {
            WebsiteVM websites = new WebsiteVM();
            try
            {
                var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
                if (string.IsNullOrEmpty(bearerToken))
                {
                    return RedirectToAction("Login", "Home");
                }

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                responseMessage = await _client.GetAsync("api/WebsiteList/");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        websites.WebsiteList = JsonConvert.DeserializeObject<List<WebsiteVM>>(response.Data.ToString()).ToList();
                    }
                }
                int domainId = await GetUserCurrentWebsiteIdAsync();       
                ViewBag.SelectedWebsiteId = domainId;
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }

            return View(websites);
        }
      

        #region Horoscope
        public async Task<IActionResult> AddHoroscope(int Id)
        {
            HoroscopeVM items = new HoroscopeVM();
            if (Id > 0)
            {
                try
                {
                    responseMessage = await _client.GetAsync("api/GetHoroscopeById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            items = JsonConvert.DeserializeObject<HoroscopeVM>(response.Data.ToString());
                        }
                    }


                }
                catch (Exception ex)
                {

                }
            }
            var categoryResponse = await _client.GetAsync("api/GetRashiList");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryData = categoryResponse.Content.ReadAsStringAsync().Result;
                var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryData);
                if (categoryResponseModel != null && categoryResponseModel.Data != null)
                {
                    items.RashiList = JsonConvert.DeserializeObject<List<RashiVM>>(categoryResponseModel.Data.ToString());
                }
            }
           return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> SaveHoroscope(HoroscopeVM model, IFormFile Image, IFormFile ThumbnailImage)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            if (Image != null && Image.Length > 0)
            {
                string blogImageName = Path.GetFileName(Image.FileName);
                var blogImageUrl = await SaveImageAsync(Image, "Blogimage", blogImageName, 1920, 1080, 300);
                model.BannerImageUrl = blogImageUrl;
            }

            if (ThumbnailImage != null && ThumbnailImage.Length > 0)
            {
                string thumbnailImageName = Path.GetFileName(ThumbnailImage.FileName);
                var thumbnailImageUrl = await SaveImageAsync(ThumbnailImage, "Thumbnails", thumbnailImageName, 1200, 620, 200);
                model.ThumbnailImageUrl = thumbnailImageUrl;
            }

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var responseMessage = await _client.PostAsync("api/AddHoroscope", content);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
            }
            else
            {
                Console.WriteLine(await responseMessage.Content.ReadAsStringAsync());
                TempData["error"] = "API error: " + responseMessage.StatusCode.ToString();
            }

            TempData["message"] = "Items saved successfully.";
            return RedirectToAction("HoroscopeList", "Admin");
        }


        [HttpGet]
        public async Task<ActionResult> DeleteHoroscope(int id)
        {
            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteHoroscope/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("HoroscopeList", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Shared");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
        }
        public async Task<ActionResult> HoroscopeList()
        {

            HoroscopeVM items = new HoroscopeVM();
            try
            {
                responseMessage = await _client.GetAsync("api/GetHoroscopeList/");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        items.HoroscopeList = JsonConvert.DeserializeObject<List<HoroscopeVM>>(response.Data.ToString()).ToList();
                    }
                }


            }

            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
            return View(items);
        }

        #endregion Horoscope

        #region Blog
        public async Task<IActionResult> AddBlog(int Id)
        {

           
            int typeId = 1;
            BlogDetailVM items = new BlogDetailVM();
            if (Id > 0)
            {
                try
                {

                    responseMessage = await _client.GetAsync("api/GetBlogDetailById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            items = JsonConvert.DeserializeObject<BlogDetailVM>(response.Data.ToString());
                        }
                    }


                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                items.WebsiteId = 2;
            }
            var categoryResponse = await _client.GetAsync($"api/GetBlogCategoryList/{typeId}");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryData = categoryResponse.Content.ReadAsStringAsync().Result;
                var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryData);
                if (categoryResponseModel != null && categoryResponseModel.Data != null)
                {
                    items.BlogCategoryList = JsonConvert.DeserializeObject<List<BlogCategoryVM>>(categoryResponseModel.Data.ToString());
                }
                else
                {
                    items.BlogCategoryList = new List<BlogCategoryVM>();
                }
            }
            else
            {
                items.BlogCategoryList = new List<BlogCategoryVM>();
            }
            return View(items);
        }
        [HttpPost]
        public async Task<IActionResult> SaveBlog(BlogDetailVM model, IFormFile Image, IFormFile AuthorImage, IFormFile ThumbnailImage)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            if (Image != null && Image.Length > 0)
            {
                string blogImageName = Path.GetFileName(Image.FileName);
                var blogImageUrl = await SaveImageAsync(Image, "Blogimage", blogImageName, 1920, 1080, 300);
                model.BannerImageUrl = blogImageUrl;
            }
            if (ThumbnailImage != null && ThumbnailImage.Length > 0)
            {
                string thumbnailImageName = Path.GetFileName(ThumbnailImage.FileName);
                var thumbnailImageUrl = await SaveImageAsync(ThumbnailImage, "Thumbnails", thumbnailImageName, 1200, 512, 200);
                model.ThumbnailImageUrl = thumbnailImageUrl;
            }
            if (AuthorImage != null && AuthorImage.Length > 0)
            {
                string authorImageName = Path.GetFileName(AuthorImage.FileName); 
                var authorImageUrl = await SaveImageAsync(AuthorImage, "AuthorImages", authorImageName, 512, 512, 200);
                model.ButtonUrl = authorImageUrl;
            }
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var responseMessage = await _client.PostAsync("api/AddBlogDetail/", content);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    var item = JsonConvert.DeserializeObject<BlogDetailVM>(response.Data.ToString());
                }
            }

            TempData["message"] = "Items saved successfully.";
            return RedirectToAction("BlogList", "Admin");



        }
        [HttpGet]
        public async Task<ActionResult> DeleteBlog(int id)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            int domainId = await GetUserCurrentWebsiteIdAsync();


            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteBlogDetail/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("BlogList", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Shared");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
        }
        public async Task<ActionResult> BlogList()
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            int domainId = await GetUserCurrentWebsiteIdAsync();


            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            int typeId = 1;
            BlogDetailVM items = new BlogDetailVM();
            try
            {
                responseMessage = await _client.GetAsync($"api/GetBlogDetailList/{typeId}/{domainId}");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        items.BlogList = JsonConvert.DeserializeObject<List<BlogDetailVM>>(response.Data.ToString()).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
            return View(items);
        }
        public async Task<IActionResult> AddBlogCategory(int Id)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            int domainId = await GetUserCurrentWebsiteIdAsync();


            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            int typeId = 1;
            BlogCategoryVM items = new BlogCategoryVM();
            if (Id > 0)
            {
                try
                {
                    responseMessage = await _client.GetAsync("api/GetBlogCategoryById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            items = JsonConvert.DeserializeObject<BlogCategoryVM>(response.Data.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                items.WebsiteId = 2;
            }
            var categoryResponse = await _client.GetAsync($"api/GetBlogCategoryList/{typeId}");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryData = categoryResponse.Content.ReadAsStringAsync().Result;
                var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryData);
                if (categoryResponseModel != null && categoryResponseModel.Data != null)
                {
                    items.BlogCategoryList = JsonConvert.DeserializeObject<List<BlogCategoryVM>>(categoryResponseModel.Data.ToString());
                }
            }


            return View(items);
        }
        [HttpPost]
        public async Task<IActionResult> SaveBlogCategory(BlogCategoryVM model)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            int domainId = await GetUserCurrentWebsiteIdAsync();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            if (ModelState.IsValid)
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                responseMessage = await _client.PostAsync("api/AddBlogCategory/", content);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        var item = JsonConvert.DeserializeObject<BlogCategoryVM>(response.Data.ToString());
                    }
                }
                TempData["message"] = "Items saved successfully.";
                return RedirectToAction("AddBlogCategory", "Admin");
            }
            return View(model);
        }
        [HttpGet]
        public async Task<ActionResult> DeleteBlogCategory(int id)
        {

            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteBlogCategory/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("AddBlogCategory", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Shared");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
        }
        public async Task<ActionResult> BlogCategoryList()
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            BlogCategoryVM items = new BlogCategoryVM();
            try
            {
                responseMessage = await _client.GetAsync("api/GetBlogCategoryList/");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        items.BlogCategoryList = JsonConvert.DeserializeObject<List<BlogCategoryVM>>(response.Data.ToString()).ToList();
                    }
                }


            }

            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
            return View(items);
        }
        #endregion Blog

        #region Rashi
        public async Task<IActionResult> AddRashi(int Id)
        {
            var model = new RashiVM();

            if (Id > 0)
            {
                var responseMessage = await _client.GetAsync("api/GetRashiById/" + Id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response?.Data != null)
                    {
                        model = JsonConvert.DeserializeObject<RashiVM>(response.Data.ToString());
                    }
                }
            }

            var categoryResponse = await _client.GetAsync("api/GetRashiList/");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryData = await categoryResponse.Content.ReadAsStringAsync();
                var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryData);
                if (categoryResponseModel?.Data != null)
                {
                    model.RashiList = JsonConvert.DeserializeObject<List<RashiVM>>(categoryResponseModel.Data.ToString());
                }
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SaveRashi(RashiVM model, IFormFile Image)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            if (Image != null && Image.Length > 0)
            {
                string rashiImageName = Path.GetFileName(Image.FileName);
                var rashiImageUrl = await SaveImageAsync(Image, "RashiImage", rashiImageName, 1920, 1080, 300);
                model.ImageUrl = rashiImageUrl;
            }
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            responseMessage = await _client.PostAsync("api/AddRashi/", content);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    var item = JsonConvert.DeserializeObject<BlogCategoryVM>(response.Data.ToString());
                }
            }
            TempData["message"] = "Items saved successfully.";
            return RedirectToAction("AddRashi", "Admin");
        }

        [HttpGet]
        public async Task<ActionResult> DeleteRashi(int id)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteRashi/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("AddRashi", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Shared");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
        }

        public async Task<ActionResult> RashiList()
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            RashiVM items = new RashiVM();
            try
            {
                responseMessage = await _client.GetAsync("api/RashiList/");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        items.RashiList = JsonConvert.DeserializeObject<List<RashiVM>>(response.Data.ToString()).ToList();
                    }
                }


            }

            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
            return View(items);
        }

        #endregion Rashi

        #region YVideo
        public async Task<IActionResult> AddYVideo(int Id)
        {
            int typeId = 3;
            BlogDetailVM items = new BlogDetailVM();
            if (Id > 0)
            {
                try
                {

                    responseMessage = await _client.GetAsync("api/GetBlogDetailById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            items = JsonConvert.DeserializeObject<BlogDetailVM>(response.Data.ToString());
                        }
                    }


                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                items.WebsiteId = 1;
            }
            var categoryResponse = await _client.GetAsync($"api/GetBlogCategoryList/{typeId}");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryData = categoryResponse.Content.ReadAsStringAsync().Result;
                var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryData);
                if (categoryResponseModel != null && categoryResponseModel.Data != null)
                {
                    items.BlogCategoryList = JsonConvert.DeserializeObject<List<BlogCategoryVM>>(categoryResponseModel.Data.ToString());
                }
            }



            return View(items);

        }

        [HttpPost]
        public async Task<IActionResult> SaveYoutubeVideo(BlogDetailVM model, IFormFile AuthorImage, IFormFile ThumbnailImage)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                TempData["error"] = "User not authorized";
                return RedirectToAction("YVideoList");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            // Upload images
            if (ThumbnailImage != null && ThumbnailImage.Length > 0)
            {
                string thumbnailName = Path.GetFileName(ThumbnailImage.FileName);
                model.ThumbnailImageUrl = await SaveImageAsync(ThumbnailImage, "Thumbnails", thumbnailName, 1200, 512, 200);
            }

            if (AuthorImage != null && AuthorImage.Length > 0)
            {
                string authorImageName = Path.GetFileName(AuthorImage.FileName);
                model.ButtonUrl = await SaveImageAsync(AuthorImage, "AuthorImages", authorImageName, 512, 512, 200);
            }


            // Call API
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var responseMessage = await _client.PostAsync("api/AddBlogDetail", content);

            if (!responseMessage.IsSuccessStatusCode)
            {
                var error = await responseMessage.Content.ReadAsStringAsync();
                TempData["error"] = "API Error: " + error;
                return View("AddYVideo", model);
            }

            TempData["message"] = "Video saved successfully.";
            return RedirectToAction("YVideoList");
        }
        public async Task<ActionResult> YVideoList()
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            int domainId = await GetUserCurrentWebsiteIdAsync();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            int typeId = 4;
            BlogDetailVM items = new BlogDetailVM();
            try
            {
                responseMessage = await _client.GetAsync($"api/GetBlogDetailList/{typeId}/{domainId}");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        items.BlogList = JsonConvert.DeserializeObject<List<BlogDetailVM>>(response.Data.ToString()).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
            return View(items);
        }

        [HttpGet]
        public async Task<ActionResult> DeleteYVideo(int Id)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteBlogDetail/{Id}");
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("YVideoList", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Shared");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
        }
        public async Task<IActionResult> AddYVideoCategory(int Id)
        {
            int typeId = 4;
            BlogCategoryVM items = new BlogCategoryVM();
            if (Id > 0)
            {
                try
                {
                    responseMessage = await _client.GetAsync("api/GetBlogCategoryById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            items = JsonConvert.DeserializeObject<BlogCategoryVM>(response.Data.ToString());
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }
            else
            {

                items.WebsiteId = 1;
            }
            var categoryResponse = await _client.GetAsync($"api/GetBlogCategoryList/{typeId}");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryData = categoryResponse.Content.ReadAsStringAsync().Result;
                var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryData);
                if (categoryResponseModel != null && categoryResponseModel.Data != null)
                {
                    items.BlogCategoryList = JsonConvert.DeserializeObject<List<BlogCategoryVM>>(categoryResponseModel.Data.ToString());
                }
            }
            return View(items);
        }
        [HttpPost]
        public async Task<IActionResult> SaveYVideoCategory(BlogCategoryVM model)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            if (ModelState.IsValid)
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                responseMessage = await _client.PostAsync("api/AddBlogCategory/", content);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        var item = JsonConvert.DeserializeObject<BlogCategoryVM>(response.Data.ToString());
                    }
                }
                TempData["message"] = "Items saved successfully.";
                return RedirectToAction("AddYVideoCategory", "Admin");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> DeleteYVideoCategory(int Id)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            try
            {
                var responseMessage = await _client.GetAsync($"api/DeleteBlogCategory/{Id}");
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["success_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("AddYVideoCategory", "Admin");
                }
                else
                {
                    var errorContent = await responseMessage.Content.ReadAsStringAsync();
                    TempData["error_message"] = $"Error deleting category: {errorContent}";
                    return RedirectToAction("Error", "Shared");
                }
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                TempData["error_message"] = $"Exception: {ex.Message}";
                return RedirectToAction("Error", "Shared");
            }
        }


        #endregion YVideo

        #region Quote
        public async Task<IActionResult> AddQuote(int Id)
        {
            int typeId = 4;
            BlogDetailVM items = new BlogDetailVM();
            if (Id > 0)
            {
                try
                {

                    responseMessage = await _client.GetAsync("api/GetBlogDetailById/" + Id);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            items = JsonConvert.DeserializeObject<BlogDetailVM>(response.Data.ToString());
                        }
                    }


                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                items.WebsiteId = 1;
            }
            var categoryResponse = await _client.GetAsync($"api/GetBlogCategoryList/{typeId}");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryData = categoryResponse.Content.ReadAsStringAsync().Result;
                var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryData);
                if (categoryResponseModel != null && categoryResponseModel.Data != null)
                {
                    items.BlogCategoryList = JsonConvert.DeserializeObject<List<BlogCategoryVM>>(categoryResponseModel.Data.ToString());
                }
            }



            return View(items);

        }
        public async Task<IActionResult> SaveQuote(BlogDetailVM model, IFormFile Image, IFormFile AuthorImage, IFormFile ThumbnailImage)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            if (Image != null && Image.Length > 0)
            {
                string blogImageName = Path.GetFileName(Image.FileName);
                var blogImageUrl = await SaveImageAsync(Image, "Blogimage", blogImageName, 1920, 1080, 300);
                model.BannerImageUrl = blogImageUrl;
            }

            if (ThumbnailImage != null && ThumbnailImage.Length > 0)
            {
                string thumbnailImageName = Path.GetFileName(ThumbnailImage.FileName);
                var thumbnailImageUrl = await SaveImageAsync(ThumbnailImage, "Thumbnails", thumbnailImageName, 1200, 512, 200);
                model.ThumbnailImageUrl = thumbnailImageUrl;
            }
            if (AuthorImage != null && AuthorImage.Length > 0)
            {
                string authorImageName = Path.GetFileName(AuthorImage.FileName);
                var authorImageUrl = await SaveImageAsync(AuthorImage, "AuthorImages", authorImageName, 512, 512, 200);
                model.ButtonUrl = authorImageUrl;
            }
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var responseMessage = await _client.PostAsync("api/AddBlogDetail/", content);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    var item = JsonConvert.DeserializeObject<BlogDetailVM>(response.Data.ToString());
                }
            }

            TempData["message"] = "Items saved successfully.";
            return RedirectToAction("QuotesList", "Admin");

        }

        public async Task<ActionResult> QuotesList()
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            int domainId = await GetUserCurrentWebsiteIdAsync();


            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            int typeId = 3;
            BlogDetailVM items = new BlogDetailVM();
            try
            {
                responseMessage = await _client.GetAsync($"api/GetBlogDetailList/{typeId}/{domainId}");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        items.BlogList = JsonConvert.DeserializeObject<List<BlogDetailVM>>(response.Data.ToString()).ToList();
                    }
                }


            }

            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
            return View(items);
        }
        [HttpGet]
        public async Task<ActionResult> DeleteQuote(int id)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteBlogDetail/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("QuotesList", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Shared");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
        }
        public async Task<IActionResult> AddQuoteCategory(int Id)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            int domainId = await GetUserCurrentWebsiteIdAsync();


            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            int typeId = 3;
            BlogCategoryVM items = new BlogCategoryVM();
            if (Id > 0)
            {
                try
                {
                    responseMessage = await _client.GetAsync("api/GetBlogCategoryById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            items = JsonConvert.DeserializeObject<BlogCategoryVM>(response.Data.ToString());
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                items.WebsiteId = 1;
            }
            var categoryResponse = await _client.GetAsync($"api/GetBlogCategoryList/{typeId}");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryData = categoryResponse.Content.ReadAsStringAsync().Result;
                var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryData);
                if (categoryResponseModel != null && categoryResponseModel.Data != null)
                {
                    items.BlogCategoryList = JsonConvert.DeserializeObject<List<BlogCategoryVM>>(categoryResponseModel.Data.ToString());
                }
            }


            return View(items);
        }
        [HttpPost]
        public async Task<IActionResult> SaveQuotesCategory(BlogCategoryVM model)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            int domainId = await GetUserCurrentWebsiteIdAsync();


            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            if (ModelState.IsValid)
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                responseMessage = await _client.PostAsync("api/AddBlogCategory/", content);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        var item = JsonConvert.DeserializeObject<BlogCategoryVM>(response.Data.ToString());
                    }
                }
                TempData["message"] = "Items saved successfully.";
                return RedirectToAction("AddQuoteCategory", "Admin");
            }

            return View(model);
        }
        public async Task<ActionResult> DeleteQuoteCategory(int id)
        {
            var bearerToken = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(bearerToken))
            {
                return RedirectToAction("Login", "Home");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteBlogCategory/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("AddQuoteCategory", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Shared");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Shared");
            }
        }
        #endregion Quote

        #region Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var emailExistsResponse = await _client.GetAsync($"api/EmailExists/{model.Email}");
                if (emailExistsResponse.IsSuccessStatusCode)
                {
                    var emailExistsData = await emailExistsResponse.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseModel>(emailExistsData);

                    if (apiResponse != null && apiResponse.StatusCode == 200 && apiResponse.Status && apiResponse.Data != null)
                    {
                        TempData["ErrorMessage"] = "Email already exists!";
                        return View(model);
                    }
                }
                var responseMessage = await _client.PostAsJsonAsync("api/registeruser/", model);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);

                    if (response?.StatusCode == 200)
                    {
                        TempData["SuccessMessage"] = "User registered successfully!";
                        return RedirectToAction("Users");
                    }

                    TempData["ErrorMessage"] = response?.Message ?? "Something went wrong";
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Shared");
            }

            return View(model);
        }

        public async Task<JsonResult> EmailAlreadyExist(string Emailline)
        {
            if (string.IsNullOrEmpty(Emailline))
            {
                return Json(new { status = false, message = "Email is not empty" });
            }

            try
            {
                var emailExistsResponse = await _client.GetAsync($"api/EmailExists/{Emailline}");

                if (emailExistsResponse.IsSuccessStatusCode)
                {
                    var emailExistsData = await emailExistsResponse.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseModel>(emailExistsData);
                    if (apiResponse != null && apiResponse.StatusCode == 200 && apiResponse.Status && apiResponse.Data != null)
                    {

                        if (apiResponse.Data.ToString().ToLower() == "true")
                        {
                            return Json(new { status = true, message = "Email already exists." });
                        }
                    }
                }

                return Json(new { status = false, message = "Email does not exist." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                return Json(new { status = false, message = "An error occurred while checking email." });
            }
        }


        public ActionResult Users()
        {
            return View();
        }

        #endregion Register


        private async Task<string> SaveImageAsync(IFormFile imageFile, string folderName, string userProvidedName, int width, int height, int maxFileSizeKB)
        {
            var extension = Path.GetExtension(userProvidedName)?.ToLower();
            var cleanedName = Path.GetFileNameWithoutExtension(userProvidedName).Replace(" ", "-").ToLower();
            var versionedNumber = Math.Round(new Random().NextDouble(), 2);
            var fileName = $"{versionedNumber}-{cleanedName}.webp";
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            var filePath = Path.Combine(directoryPath, fileName);
            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream); 
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var magickImage = new MagickImage(memoryStream))
                {
                    magickImage.Resize(new MagickGeometry((uint)Math.Max(1, width), (uint)Math.Max(1, height)));
                    magickImage.Format = MagickFormat.WebP;
                    magickImage.Settings.Interlace = Interlace.Plane;
                    magickImage.Strip();
                    magickImage.Quality = 90;
                    bool shouldReduceSize = memoryStream.Length > 1024 * 1024;
                    if (shouldReduceSize)
                    {
                        while (memoryStream.Length > maxFileSizeKB * 1024 && magickImage.Quality > 50)
                        {
                            magickImage.Quality -= 5;
                            memoryStream.SetLength(0);
                            magickImage.Write(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                        }
                    }
                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await magickImage.WriteAsync(fileStream);
                    }
                }
            }

            return $"/{folderName}/{fileName}";
        }

        [HttpPost("UploadCKImage")]
        public async Task<IActionResult> UploadCKImage(IFormFile upload)
        {
            if (upload != null && upload.Length > 0)
            {
                string folderName = "uploads";
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(upload.FileName);
                string filePath = Path.Combine(directoryPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }

                string url = $"/{folderName}/{fileName}";
                return Json(new { url });
            }
            return Json(new { error = new { message = "Failed to upload image" } });
        }

        private async Task<int> GetUserCurrentWebsiteIdAsync()
        {
            int websiteId = 0;
            try
            {                                                        
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return 0; 
                }
                var response=await _client.GetAsync($"api/GetUserCurrentWebsiteId/{userIdClaim}");               
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);

                    if (apiResponse != null && apiResponse.Status && apiResponse.Data != null)
                    {
                        websiteId = Convert.ToInt32(apiResponse.Data);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching WebsiteId: {ex.Message}");
            }

            return websiteId;
        }


        private Guid? GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            return null;
        }
        [HttpPost]      
        public async Task<JsonResult> UpdateUserWebsite([FromBody] UserWebsiteVM request)
        {
            if (request == null || request.WebsiteId == 0)
            {
                return Json(new { success = false, message = "Invalid website ID received" });
            }

            var userId = GetUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "User ID is missing" });
            }

            var requestModel = new UserWebsiteVM
            {
                UserId = userId.Value,
                WebsiteId = request.WebsiteId
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestModel), Encoding.UTF8, "application/json");
            string apiUrl = "api/UpdateUserWebsite"; 
            var responseMessage = await _client.PostAsync(apiUrl, jsonContent);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);

                return Json(new { success = true, data = response?.Data });
            }
            else
            {
                return Json(new { success = false, message = $"API call failed with status code {responseMessage.StatusCode}" });
            }
        }
        #region AddLecture


        [HttpGet]
        public async Task<ActionResult> AddLecturer(int Id)

        {
            if (!IdentityExtention.CheckSuperadminIdentity(_httpContextAccessor))
            {
                return RedirectToAction("Login", "Home");
            }
            LectureCategoryVM Lecture = new LectureCategoryVM();
            if (Id > 0)
            {
                try
                {

                    responseMessage = await _client.GetAsync("api/GetLectureById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            Lecture = JsonConvert.DeserializeObject<LectureCategoryVM>(response.Data.ToString());
                        }
                    }


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
            }


            responseMessage = await _client.GetAsync("api/GetLectureList");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    Lecture.LectureList = JsonConvert.DeserializeObject<List<LectureCategoryVM>>(response.Data.ToString()).ToList();
                }
            }
            responseMessage = await _client.GetAsync("api/GetContantMasterList");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    Lecture.ContentMasterList = JsonConvert.DeserializeObject<List<ContentMasterVM>>(response.Data.ToString()).ToList();
                }
            }
            return View(Lecture);
        }


        // Handle form submission


        [HttpPost]
        public async Task<IActionResult> AddLectureCategory(LectureCategoryVM model)
        {
            if (ModelState.IsValid)
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var responseMessage = await _client.PostAsync("api/SaveLecture/", content);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        var savedContact = JsonConvert.DeserializeObject<LectureCategoryVM>(response.Data.ToString());
                        // Optionally, you can store or log savedContact if needed
                    }

                    // Show success message
                    TempData["message"] = "Lecture saved successfully.";

                    return RedirectToAction("AddLecturer", "Admin");
                }
                else
                {
                    // Handle failed request (optional)
                    TempData["error"] = "There was an error saving the Lecture.";
                    return RedirectToAction("AddLecturer", "Admin");
                }
            }

            // If ModelState is not valid, return to the same page
            return RedirectToAction("AddLecturer", "Admin");
        }


        [HttpGet]
        public async Task<ActionResult> DeleteLecture(int id)
        {
            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteLecture/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("AddLecturer", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpGet]
        public async Task<ActionResult> AddLecturerDetail(int Id)
        {
            if (!IdentityExtention.CheckSuperadminIdentity(_httpContextAccessor))
            {
                return RedirectToAction("Login", "Home");
            }
            LectureDetailVM Lecture = new LectureDetailVM();
            if (Id > 0)
            {
                try
                {

                    responseMessage = await _client.GetAsync("api/GetLectureDetailsById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            Lecture = JsonConvert.DeserializeObject<LectureDetailVM>(response.Data.ToString());
                        }
                    }


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
            }


            responseMessage = await _client.GetAsync("api/GetLectureDetailsList");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    Lecture.LectureList = JsonConvert.DeserializeObject<List<LectureDetailVM>>(response.Data.ToString()).ToList();
                }
            }
            responseMessage = await _client.GetAsync("api/GetLectureList");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    Lecture.LectureCategoryList = JsonConvert.DeserializeObject<List<LectureCategoryVM>>(response.Data.ToString()).ToList();
                }
            }
            return View(Lecture);
        }


        // Handle form submission
        [HttpPost]
        public async Task<IActionResult> SaveLectureDetails(LectureDetailVM model)
        {
            // Check if the model state is valid
            if (ModelState.IsValid)
            {

                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                var responseMessage = await _client.PostAsync("api/AddLectureDetails/", content);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        var savedContact = JsonConvert.DeserializeObject<LectureDetailVM>(response.Data.ToString());
                    }
                    TempData["message"] = "Lecture saved successfully.";

                    return RedirectToAction("AddLecturerDetail", "Admin");
                }
                else
                {
                    // Handle failed request (optional)
                    TempData["error"] = "There was an error saving the Lecture details.";
                    return RedirectToAction("AddLecturerDetail", "Admin");
                }
            }

            // If ModelState is not valid, return to the same page
            return RedirectToAction("AddLecturerDetail", "Admin");
        }


        [HttpGet]
        public async Task<ActionResult> DeleteLectureDetail(int id)
        {
            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteLectureDetails/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("AddLecturerDetail", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }

        #endregion AddLecture
        #region Question
        [HttpGet]
        public async Task<ActionResult> AddQuestions(int Id)
        {
            if (!IdentityExtention.CheckSuperadminIdentity(_httpContextAccessor))
            {
                return RedirectToAction("Login", "Home");
            }
            QuestionVM Question = new QuestionVM();
            if (Id > 0)
            {
                try
                {

                    responseMessage = await _client.GetAsync("api/GetQuestionById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            Question = JsonConvert.DeserializeObject<QuestionVM>(response.Data.ToString());
                        }
                    }


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");

                }
            }

            responseMessage = await _client.GetAsync("api/GetQuestionList");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    Question.QuestionList = JsonConvert.DeserializeObject<List<QuestionVM>>(response.Data.ToString()).ToList();
                }
            }
            return View(Question);
        }

        [HttpPost]
        public async Task<IActionResult> SaveQuestion(QuestionVM model)
        {
            if (ModelState.IsValid)
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var responseMessage = await _client.PostAsync("api/AddQuestion/", content);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        var savedContact = JsonConvert.DeserializeObject<QuestionVM>(response.Data.ToString());
                        // Optionally, you can store or log savedContact if needed
                    }

                    // Show success message
                    TempData["message"] = "Lecture saved successfully.";

                    return RedirectToAction("AddQuestions", "Admin");
                }
                else
                {
                    // Handle failed request (optional)
                    TempData["error"] = "There was an error saving the Lecture.";
                    return RedirectToAction("AddQuestions", "Admin");
                }
            }

            // If ModelState is not valid, return to the same page
            return RedirectToAction("AddQuestions", "Admin");
        }


        [HttpGet]
        public async Task<ActionResult> DeleteQuestion(int id)
        {
            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteQuestion/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("AddQuestions", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }


        [HttpGet]
        public async Task<ActionResult> AddQuestionDetail(int Id)
        {
            if (!IdentityExtention.CheckSuperadminIdentity(_httpContextAccessor))
            {
                return RedirectToAction("Login", "Home");
            }
            QuestionDetailVM Question = new QuestionDetailVM();
            if (Id > 0)
            {
                try
                {

                    responseMessage = await _client.GetAsync("api/GetQuestionDetailById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            Question = JsonConvert.DeserializeObject<QuestionDetailVM>(response.Data.ToString());
                        }
                    }


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
            }

            responseMessage = await _client.GetAsync("api/GetQuestionDetailList");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    Question.QuestionDetailList = JsonConvert.DeserializeObject<List<QuestionDetailVM>>(response.Data.ToString()).ToList();
                }
            }
            responseMessage = await _client.GetAsync("api/GetQuestionList");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    Question.QuestionList = JsonConvert.DeserializeObject<List<QuestionVM>>(response.Data.ToString()).ToList();
                }
            }
            return View(Question);
        }

        [HttpPost]
        public async Task<IActionResult> SaveQuestionDetail(QuestionDetailVM model)
        {
            if (ModelState.IsValid)
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var responseMessage = await _client.PostAsync("api/AddQuestionDetail/", content);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        var savedContact = JsonConvert.DeserializeObject<QuestionDetailVM>(response.Data.ToString());
                        // Optionally, you can store or log savedContact if needed
                    }

                    // Show success message
                    TempData["message"] = "Lecture saved successfully.";

                    return RedirectToAction("AddQuestionDetail", "Admin");
                }
                else
                {
                    // Handle failed request (optional)
                    TempData["error"] = "There was an error saving the Lecture.";
                    return RedirectToAction("AddQuestionDetail", "Admin");
                }
            }

            // If ModelState is not valid, return to the same page
            return RedirectToAction("AddQuestionDetail", "Admin");
        }


        [HttpGet]
        public async Task<ActionResult> DeleteQuestionDetail(int id)
        {
            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteQuestionDetail/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("AddQuestionDetail", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }



        [HttpGet]
        public async Task<ActionResult> AddQucikLeran(int Id)
        {
            if (!IdentityExtention.CheckSuperadminIdentity(_httpContextAccessor))
            {
                return RedirectToAction("Login", "Home");
            }
            QuickLearnVM Question = new QuickLearnVM();
            if (Id > 0)
            {
                try
                {

                    responseMessage = await _client.GetAsync("api/GetQuickLearnById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            Question = JsonConvert.DeserializeObject<QuickLearnVM>(response.Data.ToString());
                        }
                    }


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
            }

            responseMessage = await _client.GetAsync("api/GetQuickLearnList");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    Question.QuickLearnList = JsonConvert.DeserializeObject<List<QuickLearnVM>>(response.Data.ToString()).ToList();
                }
            }
            return View(Question);
        }

        [HttpPost]
        public async Task<IActionResult> SaveQuickLearn(QuickLearnVM model)
        {
            if (ModelState.IsValid)
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var responseMessage = await _client.PostAsync("api/AddQuickLearn/", content);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        var savedContact = JsonConvert.DeserializeObject<QuickLearnVM>(response.Data.ToString());
                        // Optionally, you can store or log savedContact if needed
                    }

                    // Show success message
                    TempData["message"] = "QuickLearn saved successfully.";

                    return RedirectToAction("AddQucikLeran", "Admin");
                }
                else
                {
                    // Handle failed request (optional)
                    TempData["error"] = "There was an error saving the QuickLearn.";
                    return RedirectToAction("AddQucikLeran", "Admin");
                }
            }

            // If ModelState is not valid, return to the same page
            return RedirectToAction("AddQucikLeran", "Admin");
        }


        [HttpGet]
        public async Task<ActionResult> DeleteQuickLearn(int id)
        {
            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteQuickLearn/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("AddQucikLeran", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }
        #endregion
        #region ContantMaster

        [HttpGet]
        public async Task<ActionResult> AddHomeContent(int Id)
        {
            if (!IdentityExtention.CheckSuperadminIdentity(_httpContextAccessor))
            {
                return RedirectToAction("Login", "Home");
            }
            ContentMasterVM Question = new ContentMasterVM();
            if (Id > 0)
            {
                try
                {

                    responseMessage = await _client.GetAsync("api/GetContantMasterById/" + Id);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response != null && response.Data != null)
                        {
                            Question = JsonConvert.DeserializeObject<ContentMasterVM>(response.Data.ToString());
                        }
                    }


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
            }

            responseMessage = await _client.GetAsync("api/GetContantMasterList");
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response != null && response.Data != null)
                {
                    Question.ContentMasterList = JsonConvert.DeserializeObject<List<ContentMasterVM>>(response.Data.ToString()).ToList();
                }
            }
            return View(Question);
        }

        [HttpPost]
        public async Task<IActionResult> SaveConatentMaster(ContentMasterVM model)
        {
            if (ModelState.IsValid)
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var responseMessage = await _client.PostAsync("api/AddContantMaster/", content);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        var savedContact = JsonConvert.DeserializeObject<ContentMasterVM>(response.Data.ToString());
                        // Optionally, you can store or log savedContact if needed
                    }

                    // Show success message
                    TempData["message"] = "Data saved successfully.";

                    return RedirectToAction("AddHomeContent", "Admin");
                }
                else
                {
                    // Handle failed request (optional)
                    TempData["error"] = "There was an error saving the QuickLearn.";
                    return RedirectToAction("AddHomeContent", "Admin");
                }
            }

            // If ModelState is not valid, return to the same page
            return RedirectToAction("AddHomeContent", "Admin");
        }


        [HttpGet]
        public async Task<ActionResult> DeleteConatentMaster(int id)
        {
            try
            {
                responseMessage = await _client.GetAsync($"api/DeleteContantMaster/" + id);
                if (responseMessage.IsSuccessStatusCode)
                {
                    TempData["error_message"] = _messages.Value.DeleteSuccessfully;
                    return RedirectToAction("AddHomeContent", "Admin");
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }
        #endregion ContantMaster
    }
}
