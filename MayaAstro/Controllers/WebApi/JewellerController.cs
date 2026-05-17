using MayaAstro.DatabaseEntities;
using MayaAstro.Models;
using MayaAstro.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MayaAstro.Controllers.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class JewellerController : ControllerBase
    {
        private readonly IJewellerServices _services;

        public JewellerController(IJewellerServices services)
        {
            _services = services;
        }

        #region ================= MessageBanner (ADMIN) =================



        [HttpGet("MessageBanner/{id}")]
        public async Task<IActionResult> GetMessageBannerById(int id)
        {
            var response = await _services.GetMessageBannerDetailById(id);
            return StatusCode(response.StatusCode, response);
        }



        [HttpPut("SaveMessageBanner")]
        public async Task<IActionResult> SaveMessageBanner(MessageBannerDetailVM model)
        {
            var response = await _services.AddMessageBannerDetail(model);
            return StatusCode(response.StatusCode, response);
        }

        #endregion


        #region ================= BANK DETAIL =================

        [HttpGet("BankDetail/{id}")]
        public async Task<IActionResult> GetBankDetailById(int id)
        {
            var response = await _services.GetBankDetailById(id);
            return StatusCode(response.StatusCode, response);
        }



        [HttpPut("SaveBankDetail")]
        public async Task<IActionResult> SaveBankDetail(BankDetailVM model)
        {
            var response = await _services.AddBankDetail(model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("BankDetailList")]
        public async Task<IActionResult> GetBankDetailList()
        {
            var response = await _services.GetBankDetailList();
            return StatusCode(response.StatusCode, response);
        }


        [HttpDelete("DeleteBankDetail/{id}")]
        public async Task<IActionResult> DeleteBankDetail(int id)
        {
            var response = await _services.DeleteBankDetail(id);
            return StatusCode(response.StatusCode, response);
        }
        #endregion

        #region ================= COMMON ENQUIRY =================
        [HttpGet("verify-phone")]
        public async Task<IActionResult> VerifyPhone(string phone)
        {
            var result = await _services.VerifyPhone(phone);
            return StatusCode(result.StatusCode, result);
        }
        // Used by Register + Contact (MAUI)
        [AllowAnonymous]
        [HttpPost("enquiry")]
        public async Task<IActionResult> SaveEnquiry(CommonEnquiryVM model)
        {
            var response = await _services.AddEnquiry(model);

            if (response.StatusCode == 200)
            {
                _ = Task.Run(() => _services.SendEmailByType(model));
            }

            return StatusCode(response.StatusCode, response);
        }

        // Admin List
        [HttpGet("enquiry")]
        public async Task<IActionResult> GetEnquiryList(string type)
        {
            var response = await _services.GetEnquiryList(type);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("enquiry/{id}")]
        public async Task<IActionResult> GetEnquiryById(int id)
        {
            var response = await _services.GetEnquiryById(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("DeleteEnquiry/{id}")]
        public async Task<IActionResult> DeleteEnquiry(int id)
        {
            var response = await _services.DeleteEnquiry(id);
            return StatusCode(response.StatusCode, response);
        }

        #endregion
        [AllowAnonymous]
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent()
        {
            var data = await _services.GetCurrentGoldPriceAsync();

            return Ok(data);
        }

        [HttpGet("GetPremiumAsync/{id}")]
        public async Task<IActionResult> GetPremiumAsyncById(int id)
        {
            var response = await _services.GetPremiumAsyncById(id);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetGoldSettingsById/{id}")]
        public async Task<IActionResult> GetGoldSettingsById(int id)
        {
            var response = await _services.GetGoldSettingsById(id);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("SaveGoldSettings")]
        public async Task<IActionResult> SaveGoldSettings(GoldSettingsVM model)
        {
            var response = await _services.SaveGoldSettings(model);
            return StatusCode(response.StatusCode, response);
        }
        #region ================= ANNOUNCEMENTS =================
        [HttpPut("SaveAnnouncement")]
        public async Task<IActionResult> SaveAnnouncement(AnnouncementsVM model)
        {
            var response = await _services.SaveAnnouncement(model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("Announcement/{id}")]
        public async Task<IActionResult> GetAnnouncementById(int id)
        {
            var response = await _services.GetAnnouncementById(id);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("AnnouncementList")]
        public async Task<IActionResult> GetAnnouncementList()
        {
            var response = await _services.GetAnnouncementList();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("AnnouncementByDate")]
        public async Task<IActionResult> GetAnnouncementByDate(DateTime? startDate, DateTime? endDate)
        {
            var response = await _services.GetAnnouncementByDate(startDate, endDate);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("DeleteAnnouncement/{id}")]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var response = await _services.DeleteAnnouncement(id);
            return StatusCode(response.StatusCode, response);
        }
        #endregion
    }
}