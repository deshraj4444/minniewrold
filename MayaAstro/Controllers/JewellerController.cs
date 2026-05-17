using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MayaAstro.DatabaseEntities;
using MayaAstro.Filters;
using MayaAstro.Models;
using MayaAstro.Services.Configuration;
using MayaAstro.Services.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MayaAstro.Controllers
{
    [ServiceFilter(typeof(SuperadminAuthorizationFilter))]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class JewellerController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<Messages> _messages;
        private readonly HttpClient _client;
        HttpResponseMessage responseMessage = new HttpResponseMessage();
        private readonly IConfiguration _configuration;
        private readonly MayaAstroContext _Context;
        public IConfiguration _config { get; set; }
        public JewellerController(ILogger<HomeController> logger, IConfiguration config, IHttpContextAccessor httpContextAccessor, IOptions<Messages> messages, IConfiguration configuration, MayaAstroContext context)
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
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> AddMessageBanner()
        {
            try
            {
                MessageBannerDetailVM model = new MessageBannerDetailVM();

                    int id = 1;
                    var response = await _client.GetAsync($"api/jeweller/MessageBanner/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponseModel>(data);

                        if (result?.Data != null)
                            model = JsonConvert.DeserializeObject<MessageBannerDetailVM>(result.Data.ToString());
                    }
              

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddMessageBanner GET");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddMessageBanner(MessageBannerDetailVM model)
        {
            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(model),
                    Encoding.UTF8,
                    "application/json");

                var response = await _client.PutAsync("api/jeweller/SaveMessageBanner", content);

                if (response.IsSuccessStatusCode)
                    TempData["message"] = "MessageBanner updated successfully.";
                else
                    TempData["error"] = "Error updating MessageBanner.";

                return RedirectToAction("AddMessageBanner", new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditMessageBanner POST");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("AddMessageBanner", new { id = model.Id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddBankDetail(int id = 0)
        {
            try
            {
                BankDetailVM model = new BankDetailVM();

                // 🔹 Edit Bank Detail
                if (id > 0)
                {
                    var response = await _client.GetAsync($"api/jeweller/BankDetail/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponseModel>(data);

                        if (result?.Data != null)
                            model = JsonConvert.DeserializeObject<BankDetailVM>(result.Data.ToString());
                    }
                }

                // 🔹 Get Bank List
                var listResponse = await _client.GetAsync("api/jeweller/BankDetailList");

                if (listResponse.IsSuccessStatusCode)
                {
                    var data = await listResponse.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponseModel>(data);

                    if (result?.Data != null)
                        model.BankList = JsonConvert.DeserializeObject<List<BankDetailVM>>(result.Data.ToString());
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddBankDetail GET");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddBankDetail(BankDetailVM model, IFormFile ImageFile)
        {
            try
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/bank");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    // Save relative path in model
                    model.Image = $"/images/bank/{fileName}";
                }

                // Call your API service
                var content = new StringContent(
                    JsonConvert.SerializeObject(model),
                    Encoding.UTF8,
                    "application/json");

                var response = await _client.PutAsync("api/jeweller/SaveBankDetail", content);

                if (response.IsSuccessStatusCode)
                    TempData["message"] = "Bank detail saved successfully.";
                else
                    TempData["error"] = "Error saving bank detail.";

                return RedirectToAction("AddBankDetail");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddBankDetail POST");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("AddBankDetail");
            }
        }
        [HttpGet]
        public async Task<IActionResult> DeleteBankDetail(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"api/jeweller/DeleteBankDetail/{id}");

                if (response.IsSuccessStatusCode)
                    TempData["message"] = "Bank detail deleted successfully.";
                else
                    TempData["error"] = "Error deleting bank detail.";

                return RedirectToAction("AddBankDetail");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteBankDetail");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("AddBankDetail");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EnquiryList()
        {
            try
            {
                CommonEnquiryVM obj = new CommonEnquiryVM();

                var response = await _client.GetAsync("api/jeweller/enquiry?type=1");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponseModel>(data);

                    if (result?.Data != null)
                        obj.list = JsonConvert.DeserializeObject<List<CommonEnquiryVM>>(result.Data.ToString());
                }

                return View(obj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EnquiryList");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public async Task<IActionResult> DeleteEnquiry(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"api/jeweller/DeleteEnquiry/{id}");

                if (response.IsSuccessStatusCode)
                    TempData["message"] = "Enquiry deleted successfully.";
                else
                    TempData["error"] = "Error deleting Enquiry.";

                return RedirectToAction("EnquiryList");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteEnquiry");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("EnquiryList");
            }
        }
        [HttpGet]
        public async Task<IActionResult> RegistrationList()
        {
            try
            {
                CommonEnquiryVM obj = new CommonEnquiryVM();

                var response = await _client.GetAsync("api/jeweller/enquiry?type=2");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponseModel>(data);

                    if (result?.Data != null)
                        obj.list = JsonConvert.DeserializeObject<List<CommonEnquiryVM>>(result.Data.ToString());
                }

                return View(obj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RegistrationList");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public async Task<IActionResult> DeleteRegistration(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"api/jeweller/DeleteEnquiry/{id}");

                if (response.IsSuccessStatusCode)
                    TempData["message"] = "Registration deleted successfully.";
                else
                    TempData["error"] = "Error deleting registration.";

                return RedirectToAction("RegistrationList");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteRegistration");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("RegistrationList");
            }
        }
        [HttpGet]
        public async Task<IActionResult> AnnouncementList()
        {
            try
            {
                var model = new AnnouncementsVM();

                // Load full list
                string url = "api/jeweller/AnnouncementList";

                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponseModel>(data);

                    if (result?.Data != null)
                    {
                        model.AnnouncementsList =
                            JsonConvert.DeserializeObject<List<AnnouncementsVM>>(result.Data.ToString());
                    }
                }

                // No need to set StartDate/EndDate since it's a full list
                // model.StartDate = null;
                // model.EndDate = null;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AnnouncementList");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AnnouncementList(AnnouncementsVM model)
        {
            try
            {
                string url = "api/jeweller/AnnouncementByDate";

                List<string> parameters = new List<string>();

                if (model.StartDate.HasValue)
                    parameters.Add($"startDate={model.StartDate.Value:yyyy-MM-dd}");

                if (model.EndDate.HasValue)
                    parameters.Add($"endDate={model.EndDate.Value:yyyy-MM-dd}");

                url += "?" + string.Join("&", parameters);

                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponseModel>(data);

                    if (result?.Data != null)
                    {
                        model.AnnouncementsList =
                            JsonConvert.DeserializeObject<List<AnnouncementsVM>>(result.Data.ToString());
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AnnouncementList");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        public async Task<IActionResult> AddAnnouncement(int id = 0)
        {
            try
            {
                AnnouncementsVM model = new AnnouncementsVM();

                if (id > 0)
                {
                    var response = await _client.GetAsync($"api/jeweller/announcement/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ApiResponseModel>(data);

                        if (result?.Data != null)
                            model = JsonConvert.DeserializeObject<AnnouncementsVM>(result.Data.ToString());
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAnnouncement GET");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("AnnouncementList");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddAnnouncement(AnnouncementsVM model)
        {
            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(model),
                    Encoding.UTF8,
                    "application/json");

                var response = await _client.PutAsync("api/jeweller/SaveAnnouncement", content);

                if (response.IsSuccessStatusCode)
                    TempData["message"] = "Announcement saved successfully.";
                else
                    TempData["error"] = "Error saving announcement.";

                return RedirectToAction("AnnouncementList");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAnnouncement POST");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("AnnouncementList");
            }
        }
        [HttpGet]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"api/jeweller/DeleteAnnouncement/{id}");

                if (response.IsSuccessStatusCode)
                    TempData["message"] = "Announcement deleted successfully.";
                else
                    TempData["error"] = "Error deleting announcement.";

                return RedirectToAction("AnnouncementList");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAnnouncement");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("AnnouncementList");
            }
        }
        [HttpGet]
        public async Task<IActionResult> AddGoldSettings()
        {
            try
            {
                GoldSettingsVM model = new GoldSettingsVM();

                int id = 1;
                var response = await _client.GetAsync($"api/jeweller/GetGoldSettingsById/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponseModel>(data);

                    if (result?.Data != null)
                        model = JsonConvert.DeserializeObject<GoldSettingsVM>(result.Data.ToString());
                }


                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddGoldSettings GET");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("AddGoldSettings");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddGoldSettings(GoldSettingsVM model)
        {
            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(model),
                    Encoding.UTF8,
                    "application/json");

                var response = await _client.PutAsync("api/jeweller/SaveGoldSettings", content);

                if (response.IsSuccessStatusCode)
                    TempData["message"] = "GoldSettings updated successfully.";
                else
                    TempData["error"] = "Error updating GoldSettings.";

                return RedirectToAction("AddGoldSettings");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditGoldSettings POST");
                TempData["error"] = "Something went wrong.";
                return RedirectToAction("AddGoldSettings");
            }
        }

    }
}

