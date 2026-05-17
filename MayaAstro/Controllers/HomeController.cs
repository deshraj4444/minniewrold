 using System.Diagnostics;
using System.Net;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;
using MayaAstro.Services.Configuration;
using MayaAstro.Services.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
namespace MayaAstro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _client = new HttpClient();
            var apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
            _client.BaseAddress = new Uri(apiBaseUrl);
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("about")]
        public IActionResult About()

        {
            return View();
        }
        [Route("acharya")]
        public IActionResult Acharya()

        {
            return View();
        }

        [Route("appointments")]
        public IActionResult Appointments()
        {
            return View();
        }

        [Route("astrologers")]
        public IActionResult Astrologers()
        {
            return View();
        }

        [Route("blog/{categoryName?}")]
        public IActionResult Blog()
        {
            return View();
        }
        [Route("blogList/")]
        public async Task<JsonResult> Blog(string? categoryName, int pageSize, int pageNo, string search = "")
        {
            int domainId = 1;
            List<BlogListImageVM> items = new List<BlogListImageVM>();
            {
                //var request = _httpContextAccessor.HttpContext?.Request;
                //var domainUrl = request.Host.Value;
                //if (domainUrl.Trim() == "https://localhost:7117/".Trim())
                //{
                //    domainId = 1;
                //}
                try
                {
                    string apiUrl;
                    if (string.IsNullOrEmpty(categoryName))
                    {
                        apiUrl = $"api/BlogListImage/{pageSize}/{pageNo}/{domainId}?search={search}";
                        ViewBag.CategoryHeading = "Our Latest Blog"; 
                    }
                    else
                    {
                        var slug = categoryName.Replace("-", " ");
                        apiUrl = $"api/BlogListImage/{pageSize}/{pageNo}/{domainId}?categoryName={slug}&search={search}";
                        ViewBag.CategoryHeading = slug; // Set category heading
                    }
                    var responseMessage = await _client.GetAsync(apiUrl);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = await responseMessage.Content.ReadAsStringAsync();
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);

                        if (response?.Data != null)
                        {
                            items = JsonConvert.DeserializeObject<List<BlogListImageVM>>(response.Data.ToString());
                        }
                    }
                    else
                    {
                       // _logger.LogWarning($"API call failed with status code: {responseMessage.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, "An error occurred while fetching blog data.");
                    return Json(items);
                }
                return Json(items);
            }


        }

        [Route("{url}")]
        public async Task<ActionResult> BlogDetail(string url)
        {
            int domainId = 1;
            var slug = url.Replace("-", " ");
            BlogDetailVM blog = new BlogDetailVM();
            try
            {
                var responseMessage = await _client.GetAsync("api/GetBlogDetailByUrl/" + slug);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        blog = JsonConvert.DeserializeObject<BlogDetailVM>(response.Data.ToString());
                    }
                    else
                    {
                        return NotFound(); 
                    }
                }
                ViewBag.BlogMetaTitle = blog.BlogMetaTitle;
                ViewBag.BlogMetaDescription = blog.BlogMetaDescription;
                ViewBag.SeoTitle = blog.SeoTitle;
                ViewBag.BlogMetaKeyword = blog.BlogMetaKeyword;
                ViewBag.BlogMetaContent = blog.BlogMetaContent;

                var categoryResponse = await _client.GetAsync($"api/GetBlogCategoryUserList/{domainId}");
                if (categoryResponse.IsSuccessStatusCode)
                {
                    var categoryResponseData = categoryResponse.Content.ReadAsStringAsync().Result;
                    var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryResponseData);
                    if (categoryResponseModel != null && categoryResponseModel.Data != null)
                    {
                        blog.BlogCategoryList = JsonConvert.DeserializeObject<List<BlogCategoryVM>>(categoryResponseModel.Data.ToString());
                    }
                }
                var blogListResponse = await _client.GetAsync($"api/HomeBlogList/{domainId}");
                if (blogListResponse.IsSuccessStatusCode)
                {
                    var categoryResponseData = blogListResponse.Content.ReadAsStringAsync().Result;
                    var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryResponseData);
                    if (categoryResponseModel != null && categoryResponseModel.Data != null)
                    {
                        blog.BlogList = JsonConvert.DeserializeObject<List<BlogDetailVM>>(categoryResponseModel.Data.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return View(blog);
        }

        [Route("contact")]
        public IActionResult Contact()
        {
            return View();
        }

       



        public async Task<IActionResult> Index()
        {
            var blogDetails = await GetBlogDetails();
            var hrresponse = await GetLocalHoroscope();
            if (blogDetails == null)
            {
                ViewBag.Error = "Blog details could not be retrieved.";
                return View(new IndexPageVM());
            }

            if (hrresponse == null)
            {
                ViewBag.Error = "Horoscope details could not be retrieved.";
                return View(new IndexPageVM());
            }
            string[] signs = {
        "Aries", "Taurus", "Gemini", "Cancer", "Leo",
        "Virgo", "Libra", "Scorpio", "Sagittarius",
        "Capricorn", "Aquarius", "Pisces"
    };
            string horoscopeBaseUrl = "https://horoscope-app-api.vercel.app/";
            var horoscopeTasks = signs.Select(sign =>
      _client.GetAsync($"{horoscopeBaseUrl}api/v1/get-horoscope/daily?sign={sign}&day=TOMORROW")
  ).ToList();
            await Task.WhenAll(horoscopeTasks);

            var horoscopes = new List<HoroscopeResponse>();
            foreach (var task in horoscopeTasks)
            {
                var responseMessage = await task;
                if (responseMessage.IsSuccessStatusCode)
                {
                    var apiResponse = await responseMessage.Content.ReadFromJsonAsync<ApiResponse<HoroscopeResponse>>();
                    if (apiResponse?.Data != null)
                    {
                        horoscopes.Add(apiResponse.Data);
                    }
                }
                else
                {

                    ViewBag.Error = "Some horoscope data could not be retrieved.";
                }
            }
            



            var viewModel = new IndexPageVM
            {
                Horoscopes = horoscopes,
                BlogList = blogDetails,
                HoroscopeVM = hrresponse
            };

            return View(viewModel);
        }

        [Route("privacy-policy")]
        public IActionResult Privacy()
        {
            return View();
        }


        [Route("quotes/{categoryName?}")]
        public  IActionResult Quote()
        {
            return View();
        }
        [Route("quoteList/")]
        public async Task<JsonResult> Quote(string? categoryName, int pageSize, int pageNo, string search = "")
        {
            int domainId = 1;


            List<BlogListImageVM> items = new List<BlogListImageVM>();
            {
                //var request = _httpContextAccessor.HttpContext?.Request;
                //var domainUrl = request.Host.Value;
                //if (domainUrl.Trim() == "https://localhost:7117/".Trim())
                //{
                //    domainId = 1;
                //}
                try
                {
                    string apiUrl;
                    if (string.IsNullOrEmpty(categoryName))
                    {
                        apiUrl = $"api/BlogListImage/{pageSize}/{pageNo}/{domainId}?search={search}";
                        ViewBag.CategoryHeading = "Our Latest Blog";
                    }
                    else
                    {
                        var slug = categoryName.Replace("-", " ");
                        apiUrl = $"api/BlogListImage/{pageSize}/{pageNo}/{domainId}?categoryName={slug}&search={search}";
                        ViewBag.CategoryHeading = slug; // Set category heading
                    }
                    // Call the API
                    var responseMessage = await _client.GetAsync(apiUrl);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = await responseMessage.Content.ReadAsStringAsync();
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);

                        if (response?.Data != null)
                        {
                            items = JsonConvert.DeserializeObject<List<BlogListImageVM>>(response.Data.ToString());
                        }
                    }
                    else
                    {
                      //  _logger.LogWarning($"API call failed with status code: {responseMessage.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, "An error occurred while fetching blog data.");
                    return Json(items);
                }
                return Json(items);
            }
        }

        [Route("quote/{url}")]
        public async Task<ActionResult> QuoteDetail(string url)
        {
            int domainId = 1;
            var slug = url.Replace("-", " ");
            BlogDetailVM blog = new BlogDetailVM();
            try
            {
                var responseMessage = await _client.GetAsync("api/GetBlogDetailByUrl/" + slug);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        blog = JsonConvert.DeserializeObject<BlogDetailVM>(response.Data.ToString());
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                ViewBag.BlogMetaTitle = blog.BlogMetaTitle;
                ViewBag.BlogMetaDescription = blog.BlogMetaDescription;
                ViewBag.SeoTitle = blog.SeoTitle;
                ViewBag.BlogMetaKeyword = blog.BlogMetaKeyword;
                ViewBag.BlogMetaContent = blog.BlogMetaContent;

                var categoryResponse = await _client.GetAsync($"api/fGetBlogCategoryUserList/{domainId}");
                if (categoryResponse.IsSuccessStatusCode)
                {
                    var categoryResponseData = categoryResponse.Content.ReadAsStringAsync().Result;
                    var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryResponseData);
                    if (categoryResponseModel != null && categoryResponseModel.Data != null)
                    {
                        blog.BlogCategoryList = JsonConvert.DeserializeObject<List<BlogCategoryVM>>(categoryResponseModel.Data.ToString());
                    }
                }
                var blogListResponse = await _client.GetAsync($"api/HomeBlogList/{domainId}");
                if (blogListResponse.IsSuccessStatusCode)
                {
                    var categoryResponseData = blogListResponse.Content.ReadAsStringAsync().Result;
                    var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryResponseData);
                    if (categoryResponseModel != null && categoryResponseModel.Data != null)
                    {
                        blog.BlogList = JsonConvert.DeserializeObject<List<BlogDetailVM>>(categoryResponseModel.Data.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return View(blog);
        }

        [Route("services")]
        public IActionResult Services()
        {
            return View();
        }
       
        [Route("shop")]
        public IActionResult Shop()
        {
            return View();
        }
        [Route("term-of-use")]
        public IActionResult TermOfUse()

        {
            return View();
        }

        [Route("videos/{categoryName?}")]
        public IActionResult Video()
        {
            return View();
        }
        [Route("videoList/")]
        public async Task<JsonResult> Video(string? categoryName, int pageSize, int pageNo, string search = "")
        {
            int domainId = 1;
            int typeId = 4;

            List<BlogListImageVM> items = new List<BlogListImageVM>();
            {
                //var request = _httpContextAccessor.HttpContext?.Request;
                //var domainUrl = request.Host.Value;
                //if (domainUrl.Trim() == "https://localhost:7117/".Trim())
                //{
                //    domainId = 1;
                //}
                try
                {
                    string apiUrl;
                    if (string.IsNullOrEmpty(categoryName))
                    {
                        apiUrl = $"api/BlogListImage/{pageSize}/{pageNo}/{domainId}/{typeId}?search={search}";
                        ViewBag.CategoryHeading = "Our Latest Videos";
                    }
                    else
                    {
                        var slug = categoryName.Replace("-", " ");
                        apiUrl = $"api/BlogListImage/{pageSize}/{pageNo}/{domainId}/{typeId}?categoryName={slug}&search={search}";
                        ViewBag.CategoryHeading = slug;
                    }
                    var responseMessage = await _client.GetAsync(apiUrl);
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var responseData = await responseMessage.Content.ReadAsStringAsync();
                        var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                        if (response?.Data != null)
                        {
                            items = JsonConvert.DeserializeObject<List<BlogListImageVM>>(response.Data.ToString());
                        }
                    }
                    else
                    {
                     //   _logger.LogWarning($"API call failed with status code: {responseMessage.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex, "An error occurred while fetching blog data.");
                    return Json(items);
                }
                return Json(items);
            }
        }
        [Route("video/{url}")]
        public async Task<ActionResult> VideoDetail(string url)
        {
            int domainId = 1;
            int typeId = 3;
            var slug = url.Replace("-", " ");
            BlogDetailVM blog = new BlogDetailVM();
            try
            {
                var responseMessage = await _client.GetAsync("api/GetBlogDetailByUrl/" + slug);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        blog = JsonConvert.DeserializeObject<BlogDetailVM>(response.Data.ToString());
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                ViewBag.BlogMetaTitle = blog.BlogMetaTitle;
                ViewBag.BlogMetaDescription = blog.BlogMetaDescription;
                ViewBag.SeoTitle = blog.SeoTitle;
                ViewBag.BlogMetaKeyword = blog.BlogMetaKeyword;
                ViewBag.BlogMetaContent = blog.BlogMetaContent;

                var categoryResponse = await _client.GetAsync($"api/GetBlogCategoryUserList/{domainId}/{typeId}");
                if (categoryResponse.IsSuccessStatusCode)
                {
                    var categoryResponseData = categoryResponse.Content.ReadAsStringAsync().Result;
                    var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryResponseData);
                    if (categoryResponseModel != null && categoryResponseModel.Data != null)
                    {
                        blog.BlogCategoryList = JsonConvert.DeserializeObject<List<BlogCategoryVM>>(categoryResponseModel.Data.ToString());
                    }
                }
                var blogListResponse = await _client.GetAsync($"api/HomeBlogList/{domainId}/{typeId}");
                if (blogListResponse.IsSuccessStatusCode)
                {
                    var categoryResponseData = blogListResponse.Content.ReadAsStringAsync().Result;
                    var categoryResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(categoryResponseData);
                    if (categoryResponseModel != null && categoryResponseModel.Data != null)
                    {
                        blog.BlogList = JsonConvert.DeserializeObject<List<BlogDetailVM>>(categoryResponseModel.Data.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return View(blog);
        }


        [Route("zodiac/{sign}")]
        public async Task<IActionResult> Zodiac(string sign)
        {
            if (string.IsNullOrWhiteSpace(sign))
            {
                return RedirectToAction("Index");
            }
            string horoscopeBaseUrl = "https://horoscope-app-api.vercel.app/";
            string url = $"{horoscopeBaseUrl}api/v1/get-horoscope/daily?sign={sign}&day=TOMORROW";
            var apiResponse = await _client.GetAsync(url);
            HoroscopeResponse horoscopeData = null;
            if (apiResponse.IsSuccessStatusCode)
            {
                var apiResponseData = await apiResponse.Content.ReadFromJsonAsync<ApiResponse<HoroscopeResponse>>();
                if (apiResponseData?.Data != null)
                {
                    horoscopeData = apiResponseData.Data;
                }
            }
            HoroscopeVM horoscope = new HoroscopeVM();
            var slug = Uri.EscapeDataString(sign.Replace("-", " "));
            var hresponseMessage = await _client.GetAsync($"api/GetHoroscopeDetailByUrl/{slug}");
            if (hresponseMessage.IsSuccessStatusCode)
            {
                var hresponseData = await hresponseMessage.Content.ReadAsStringAsync();
                var hresponse = JsonConvert.DeserializeObject<ApiResponseModel>(hresponseData);
                if (hresponse?.Data != null)
                {
                    horoscope = JsonConvert.DeserializeObject<HoroscopeVM>(hresponse.Data.ToString());
                }
            }
            var horoscopeListResponse = await _client.GetAsync("api/GetRashiList/");
            if (horoscopeListResponse.IsSuccessStatusCode)
            {
                var horoscopeResponseData = horoscopeListResponse.Content.ReadAsStringAsync().Result;
                var horoscopeResponseModel = JsonConvert.DeserializeObject<ApiResponseModel>(horoscopeResponseData);
                if (horoscopeResponseModel != null && horoscopeResponseModel.Data != null)
                {
                    horoscope.RashiList = JsonConvert.DeserializeObject<List<RashiVM>>(horoscopeResponseModel.Data.ToString());
                }
            }

            ViewBag.Sign = sign;
            var combinedData = new ZodiacCombinedViewModelVM
            {
                HoroscopeData = horoscopeData,
                HoroscopeVMData = horoscope
            };

            return View(combinedData);
        }

        
        private async Task<List<BlogDetailVM>> GetBlogDetails()
        {
            int domainId = 1;
            try
            {
                var responseMessage = await _client.GetAsync($"api/HomeBlogList/{domainId}");

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response?.Data != null)
                    {
                        var allBlogs = JsonConvert.DeserializeObject<List<BlogDetailVM>>(response.Data.ToString()).ToList();
                        return allBlogs.Take(3).ToList();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        private async Task<List<HoroscopeVM>> GetLocalHoroscope()
        {

            try
            {
                var responseMessage = await _client.GetAsync("api/GetHoroscopeList/");

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response?.Data != null)
                    {
                        var allBlogs = JsonConvert.DeserializeObject<List<HoroscopeVM>>(response.Data.ToString()).ToList();
                        return allBlogs;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }



        #region Login
        [Route("login")]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Check if the logged-in user is the static admin
                var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(emailClaim) && emailClaim.ToLower() == "jjpadmin@gmail.com")
                {
                    // Redirect static admin to Bank Detail page
                    return RedirectToAction("AddBankDetail", "Jeweller");
                }

                // Default redirect for normal authenticated users
                return RedirectToAction("Index", "Admin");
            }

            // If not logged in, show the login page
            return View();
        }
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM login)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json");
                var responseMessage = await _client.PostAsync("api/LoginApi/", content);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Login failed. Please try again.");
                    return View(login);
                }

                var responseData = await responseMessage.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseData))
                {
                    ModelState.AddModelError(string.Empty, "You have entered an incorrect email or password.");
                    return View(login);
                }

                var loginViewModel = JsonConvert.DeserializeObject<LoginVM>(responseData);
                if (loginViewModel == null || string.IsNullOrEmpty(loginViewModel.AuthToken))
                {
                    ModelState.AddModelError(string.Empty, "Failed to retrieve authentication token.");
                    return View(login);
                }

                _httpContextAccessor.HttpContext.Session.SetString("AuthToken", loginViewModel.AuthToken);
                int websiteId = 0;
                var websiteResponse = await _client.GetAsync($"api/GetUserCurrentWebsiteId/{loginViewModel.Id}");

                if (websiteResponse.IsSuccessStatusCode)
                {
                    var websiteApiResponse = await websiteResponse.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(websiteApiResponse))
                    {
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponseModel>(websiteApiResponse);
                        if (apiResponse?.Status == true && apiResponse.Data != null)
                        {
                            int.TryParse(apiResponse.Data.ToString(), out websiteId);
                        }
                    }
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginViewModel.Id.ToString()),
                    new Claim(ClaimTypes.GivenName, loginViewModel.FirstName ?? ""),
                    new Claim(ClaimTypes.Email, loginViewModel.Email ?? ""),
                    new Claim(ClaimTypes.Role, loginViewModel.UserType.ToString()),
                    new Claim("WebsiteId", websiteId.ToString()),
                    new Claim("AuthToken", loginViewModel.AuthToken)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var props = new AuthenticationProperties
                {
                    IsPersistent = true,                      // persistent cookie
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60) // expires in 60 minutes
                };
                await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

                // ✅ Redirect static admin to BankDetail first
                if (loginViewModel.Email.ToLower() == "jjpadmin@gmail.com")
                {
                    return RedirectToAction("AddBankDetail", "Jeweller");
                }

                // Default redirect for normal users
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(login);
            }
        }

        #endregion Login

        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                _httpContextAccessor.HttpContext.Session.Clear(); // Clear session data if needed

                return RedirectToAction("Login", "Home"); // Ensure a return statement exists
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                return RedirectToAction("Error", "Home"); // Handle errors gracefully
            }
        }



        //[Route("email-parsing")]
        //public IActionResult EmailParsing()
        //{
        //    string parsedEmailData = string.Empty;

        //    try
        //    {
        //        // Path to your Python script
        //        string pythonScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "PythonScripts", "email_parser.py");

        //        // Ensure the Python executable path is correct. If it's not found, use full path to Python executable.
        //        string pythonExe = "python"; // or "python3" based on your setup

        //        // Call Python script using Process
        //        var startInfo = new ProcessStartInfo
        //        {
        //            FileName = pythonExe,  // python or python3 (verify which one works for you)
        //            Arguments = $"\"{pythonScriptPath}\" \"This is the body of the email.\"", // Passing email body content as argument
        //            RedirectStandardOutput = true,
        //            RedirectStandardError = true, // Capture errors as well
        //            UseShellExecute = false,
        //            CreateNoWindow = true
        //        };

        //        using (var process = Process.Start(startInfo))
        //        {
        //            using (var reader = process.StandardOutput)
        //            {
        //                parsedEmailData = reader.ReadToEnd();  // Read the output of the Python script
        //            }

        //            // Capture any potential error output
        //            using (var errorReader = process.StandardError)
        //            {
        //                string error = errorReader.ReadToEnd();
        //                if (!string.IsNullOrEmpty(error))
        //                {
        //                    _logger.LogError("Python Script Error: " + error);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error parsing email: {ex.Message}");
        //        parsedEmailData = "An error occurred while parsing the email.";
        //    }

        //    // Return the parsed email data to the view
        //    return View("EmailParsing", model: parsedEmailData);
        //}
    }

}




