using System.Text;
using System.Text.Json;
using MayaAstro.DatabaseEntities;
using MayaAstro.Models;
using MayaAstro.Services.Repositories;

namespace MayaAstro.Services.Services
{
    public class JewellerServices : IJewellerServices
    {
        private readonly IJewellerRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public JewellerServices(HttpClient httpClient,
            IJewellerRepository repository,
            IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

        }

        #region Admin Methods



        public async Task<ApiResponseModel> GetMessageBannerDetailById(int id)
        {
            return await _repository.GetMessageBannerDetailById(id);
        }
        public async Task<ApiResponseModel> AddMessageBannerDetail(MessageBannerDetailVM model)

        {
            return await _repository.AddMessageBannerDetail(model);
        }

        #endregion



        #region BankDetail

        public async Task<ApiResponseModel> GetBankDetailById(int id)
        {
            return await _repository.GetBankDetailById(id);
        }

        public async Task<ApiResponseModel> AddBankDetail(BankDetailVM model)
        {
            return await _repository.AddBankDetail(model);
        }
        public async Task<ApiResponseModel> GetBankDetailList()
        {
            return await _repository.GetBankDetailList();

        }
        public async Task<ApiResponseModel> DeleteBankDetail(int id)
        {
            return await _repository.DeleteBankDetail(id);

        }
        #endregion
        #region CommonEnquiry CRUD
        public async Task<ApiResponseModel> VerifyPhone(string phone)
        {
            return await _repository.VerifyPhone(phone);
        }
        public async Task<ApiResponseModel> AddEnquiry(CommonEnquiryVM model)
        {
            return await _repository.AddEnquiry(model);
        }


public async Task SendEmailByType(CommonEnquiryVM model)
    {
        try
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Type))
                return;

            if (!int.TryParse(model.Type.Trim(), out int type))
                return;

            var apiKey = _configuration["BrevoSettings:ApiKey"];
            var senderEmail = _configuration["BrevoSettings:SenderEmail"];
            var senderName = _configuration["BrevoSettings:SenderName"];
            var adminEmail = _configuration["BrevoSettings:AdminEmail"];

            string subject;
            string htmlContent;

            // Get the full path using wwwroot
            var templatesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates");

            switch (type)
            {
                case 1: // Enquiry
                    subject = "New Enquiry Received - JJP";
                    htmlContent = File.ReadAllText(Path.Combine(templatesPath, "EnquiryEmailTemplate.html"))
                        .Replace("{{Name}}", model.Name ?? "")
                        .Replace("{{Phone}}", model.Phone ?? "")
                        .Replace("{{Email}}", model.Email ?? "")
                        .Replace("{{Subject}}", model.Subject ?? "")
                        .Replace("{{Message}}", model.Message ?? "");
                    break;

                case 2: // Registration
                    subject = "New User Registration - JJP";
                    htmlContent = File.ReadAllText(Path.Combine(templatesPath, "RegistrationEmailTemplate.html"))
                        .Replace("{{Name}}", model.Name ?? "")
                        .Replace("{{FirmName}}", model.FirmName ?? "")
                        .Replace("{{Phone}}", model.Phone ?? "")
                        .Replace("{{City}}", model.City ?? "");
                    break;

                default:
                    return;
            }

            var emailData = new
            {
                sender = new { email = senderEmail, name = senderName },
                to = new[] { new { email = adminEmail } },
                subject,
                htmlContent
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(emailData),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Brevo Error: " + error);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Email Sending Failed: " + ex.Message);
        }
    }
    public async Task<ApiResponseModel> DeleteEnquiry(int id)
        {
            return await _repository.DeleteEnquiry(id);
        }

        public async Task<ApiResponseModel> GetEnquiryById(int id)
        {
            return await _repository.GetEnquiryById(id);
        }

        public async Task<ApiResponseModel> GetEnquiryList(string type)
        {
            return await _repository.GetEnquiryList(type);
        }

        #endregion
        public async Task<GoldPriceDataVM> GetCurrentGoldPriceAsync()
        {
            decimal usdInr = 0;
            decimal gold10g = 0;
            decimal silverPerKg = 0;

            try
            {
                // 1️⃣ USD → INR
                var inrJson = await _httpClient.GetStringAsync("https://open.er-api.com/v6/latest/USD");
                var inrData = JsonSerializer.Deserialize<CurrencyResponse>(inrJson);
                usdInr = inrData.rates.INR;

                // 2️⃣ GOLD price (per ounce → 10g)
                var goldJson = await _httpClient.GetStringAsync("https://api.gold-api.com/price/XAU");
                var gold = JsonSerializer.Deserialize<MetalPrice>(goldJson);
                decimal goldPerGram = (gold.price / 31.1035m) * usdInr;
                gold10g = goldPerGram * 10;

                // 3️⃣ SILVER price (per ounce → per kg)
                var silverJson = await _httpClient.GetStringAsync("https://api.gold-api.com/price/XAG");
                var silver = JsonSerializer.Deserialize<MetalPrice>(silverJson);
                decimal silverPerGram = (silver.price / 31.1035m) * usdInr;
                silverPerKg = silverPerGram * 1000;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching metal prices: " + ex.Message);
            }

            // 4️⃣ Get premium from repository (both Gold & Silver together)
            decimal goldPremium = 0;
            decimal silverPremium = 0;

            var premiumResponse = await _repository.GetPremiumAsyncById(1); // Single call now returns both Gold & Silver
            if (premiumResponse != null && premiumResponse.Status && premiumResponse.Data != null)
            {
                // Deserialize Data as a dynamic object
                var premiumObj = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(premiumResponse.Data));

                goldPremium = premiumObj.GetProperty("GoldPremium").GetDecimal();
                silverPremium = premiumObj.GetProperty("SilverPremium").GetDecimal();
            }

            // 5️⃣ Dealer base prices including premium
            decimal goldDealerBase = gold10g + goldPremium;
            decimal silverDealerBase = silverPerKg + silverPremium;

            // 6️⃣ GST only for gold
            decimal goldWithGst = goldDealerBase * 1.03m;

            // 7️⃣ Futures / Next
            decimal goldFuture = goldDealerBase + 800;
            decimal goldNext = goldDealerBase + 1200;

            decimal silverFuture = silverDealerBase + 5000;
            decimal silverNext = silverDealerBase + 7000;

            // 8️⃣ Dealer / Rate board calculations
            var result = new GoldPriceDataVM
            {
                InrSpot = usdInr,
                GoldSpot = goldDealerBase,
                SilverSpot = silverDealerBase,

                Gold995InclGstBuy = goldWithGst - 150,
                Gold995InclGstSell = goldWithGst + 150,

                Gold995Below50Buy = goldWithGst - 120,
                Gold995Below50Sell = goldWithGst + 180,

                Gold995Below10Buy = goldWithGst - 100,
                Gold995Below10Sell = goldWithGst + 200,

                GoldFutureBuy = goldFuture - 100,
                GoldFutureSell = goldFuture + 100,

                GoldNextBuy = goldNext - 100,
                GoldNextSell = goldNext + 100,

                Silver30KgImportedBuy = silverDealerBase - 2000,
                Silver30KgImportedSell = silverDealerBase + 2000,

                SilverBelow30KgBuy = silverDealerBase - 1500,
                SilverBelow30KgSell = silverDealerBase + 2500,

                SilverFutureBuy = silverFuture - 1500,
                SilverFutureSell = silverFuture + 1500,

                SilverNextBuy = silverNext - 1500,
                SilverNextSell = silverNext + 1500,

                UpdatedAt = DateTime.UtcNow
            };

            // 9️⃣ Save to DB
            await _repository.SaveGoldPriceAsync(new GoldPriceData
            {
                InrSpot = result.InrSpot,
                GoldSpot = result.GoldSpot,
                SilverSpot = result.SilverSpot,

                Gold995InclGstBuy = result.Gold995InclGstBuy,
                Gold995InclGstSell = result.Gold995InclGstSell,

                Gold995Below50Buy = result.Gold995Below50Buy,
                Gold995Below50Sell = result.Gold995Below50Sell,

                Gold995Below10Buy = result.Gold995Below10Buy,
                Gold995Below10Sell = result.Gold995Below10Sell,

                Silver30KgImportedBuy = result.Silver30KgImportedBuy,
                Silver30KgImportedSell = result.Silver30KgImportedSell,

                SilverBelow30KgBuy = result.SilverBelow30KgBuy,
                SilverBelow30KgSell = result.SilverBelow30KgSell,

                GoldFutureBuy = result.GoldFutureBuy,
                GoldFutureSell = result.GoldFutureSell,

                SilverFutureBuy = result.SilverFutureBuy,
                SilverFutureSell = result.SilverFutureSell,

                GoldNextBuy = result.GoldNextBuy,
                GoldNextSell = result.GoldNextSell,

                SilverNextBuy = result.SilverNextBuy,
                SilverNextSell = result.SilverNextSell,

                UpdatedAt = DateTime.UtcNow
            });

            return result;
        }
        public async Task<ApiResponseModel> SaveAnnouncement(AnnouncementsVM model)
        {
            return await _repository.SaveAnnouncement(model);

        }

        public async Task<ApiResponseModel> GetAnnouncementList()
        {
            return await _repository.GetAnnouncementList();
        }

        public async Task<ApiResponseModel> GetAnnouncementById(int id)
        {
            return await _repository.GetAnnouncementById(id);
        }


        public async Task<ApiResponseModel> GetAnnouncementByDate(DateTime? startDate, DateTime? endDate)
        {
            return await _repository.GetAnnouncementByDate(startDate, endDate);
        }

        public async Task<ApiResponseModel> DeleteAnnouncement(int id)
        {
            return await _repository.DeleteAnnouncement(id);
        }

        public async Task<ApiResponseModel> SaveGoldSettings(GoldSettingsVM model)
        {
            return await _repository.SaveGoldSettings(model);
        }
        public async Task<ApiResponseModel> GetPremiumAsyncById(int Id)
        {
            return await _repository.GetPremiumAsyncById(Id);
        }
        public async Task<ApiResponseModel> GetGoldSettingsById(int Id)
        {
            return await _repository.GetGoldSettingsById(Id);
        }

    }
}