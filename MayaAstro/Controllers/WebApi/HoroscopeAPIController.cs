using MayaAstro.Models;
using MayaAstro.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MayaAstro.Controllers.WebApi
{
    [ApiController]

    public class HoroscopeApiController : ControllerBase
    {
        private readonly IHoroscopeService _iHoroscopeService;

        public HoroscopeApiController(IHoroscopeService iHoroscopeService)
        {
            _iHoroscopeService = iHoroscopeService;
        }

        #region Rashi Type

        [HttpGet]
        [Route("api/GetRashiList")]
        public async Task<IActionResult> GetRashiList()
        {
            var data = await _iHoroscopeService.GetRashiList();
            return Ok(data);
        }

        [HttpPost]
        [Authorize]
        [Route("api/AddRashi")]
        public async Task<IActionResult> AddRashi(RashiVM obj)
        {
            var data = await _iHoroscopeService.AddRashi(obj);
            return Ok(data);
        }

        [HttpGet]
        [Route("api/GetRashiById/{id}")]
        public async Task<IActionResult> GetRashiById(int id)
        {
            var data = await _iHoroscopeService.GetRashiById(id);
            return Ok(data);
        }
        [HttpGet]
        [Route("api/DeleteRashi/{id}")]
        public async Task<IActionResult> DeleteRashi(int id)
        {
            var data = await _iHoroscopeService.DeleteRashi(id);
            return Ok(data);
        }

        #endregion Rashi Type

        #region Horoscope Detail

        [HttpGet]
        [Route("api/GetHoroscopeList")]
        public async Task<IActionResult> GetHoroscopeList()
        {
            var data = await _iHoroscopeService.GetHoroscopeList();
            return Ok(data);
        }



        [HttpPost]
        [Authorize]
        [Route("api/AddHoroscope")]
        public async Task<IActionResult> AddHoroscope([FromBody] HoroscopeVM obj)
        {
            var data = await _iHoroscopeService.AddHoroscope(obj);
            return Ok(data);
        }

        [HttpGet]
        [Route("api/GetHoroscopeById/{id}")]
        public async Task<IActionResult> GetHoroscopeById(int id)
        {
            var data = await _iHoroscopeService.GetHoroscopeById(id);
            return Ok(data);
        }

        [HttpGet]
        [Route("api/DeleteHoroscope/{id}")]
        public async Task<IActionResult> DeleteHoroscope(int id)
        {
            var data = await _iHoroscopeService.DeleteHoroscope(id);
            return Ok(data);
        }


        [HttpGet]
        [Route("api/GetHoroscopeDetailByUrl/{slug}")]
        public async Task<IActionResult> GetHoroscopeDetailByUrl(string slug)
        {
            var data = await _iHoroscopeService.GetHoroscopeDetailByUrl(slug);
            return Ok(data);
        }


        [HttpGet]
        [Route("api/getdata")]
        public async Task<IActionResult> TrueFalse()
        {
            var data = await _iHoroscopeService.TrueFalse();
            return Ok(data.Message);
        }

        #endregion Horoscope Detail
    }
}
