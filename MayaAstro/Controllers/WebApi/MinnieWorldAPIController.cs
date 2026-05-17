using MayaAstro.Models;
using MayaAstro.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace MayaAstro.Controllers.WebApi
{

  [ApiController]
    [AllowAnonymous]
    public class MinnieWorldAPIController : ControllerBase
    {

        private readonly IMinnieWorldServices _iminnieWorldService;

        public MinnieWorldAPIController(IMinnieWorldServices iminnieWorldService)
        {
            _iminnieWorldService = iminnieWorldService;
        }
        #region Contact

        [HttpGet]
        [Route("api/GetContactList/")]
        public async Task<IActionResult> GetContactList()
        {
            var data = await _iminnieWorldService.GetContactList();
            return Ok(data);
        }


        [HttpPost]
        [Route("api/AddContact/")]
        public async Task<IActionResult> AddContact(ContactsVM obj)
        {
            var data = await _iminnieWorldService.AddContact(obj);

            return Ok(data);
        }

        [HttpGet]
        [Route("api/GetContactById/{Id}")]
        public async Task<IActionResult> GetContactById(int Id)
        {
            var data = await _iminnieWorldService.GetContactById(Id);
            return Ok(data);
        }
        [HttpGet]
        [Route("api/DeleteContact/{Id}")]
        public async Task<IActionResult> DeleteContact(int Id)
        {
            var data = await _iminnieWorldService.DeleteContact(Id);
            return Ok(data);
        }

        #endregion Contact
    }
}
