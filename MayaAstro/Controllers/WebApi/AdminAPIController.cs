using MayaAstro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MayaAstro.Services.Services;

namespace MayaAstro.Controllers.WebApi
{
    [ApiController]
    [Authorize]
    public class AdminAPIController : ControllerBase
    {
        private readonly ILoginServices _iLoginServices;

        #region Users
        [HttpGet]
        [Route("api/GetUserList/")]
        public async Task<IActionResult> GetUserList()
        {
            var data = await _iLoginServices.GetUserList();
            return Ok(data);
        }
        [HttpGet]
        [Route("api/GetUserById/{Id}")]
        public async Task<IActionResult> GetUserById(Guid Id)
        {
            var data = await _iLoginServices.GetUserById(Id);
            return Ok(data);
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("api/registeruser/")]
        public async Task<IActionResult> RegisterUser(UserVM users)
        {
            var data = await _iLoginServices.RegisterUser(users);
            return Ok(data);
        }
        [HttpGet]
        [Route("api/DeleteUser/{Id}")]
        public async Task<IActionResult> DeleteUser(Guid Id)
        {
            var data = await _iLoginServices.DeleteUser(Id);
            return Ok(data);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("api/EmailExists/{email}")]
        public async Task<IActionResult> EmailExists(string email)
        {
            var data = await _iLoginServices.EmailExists(email);
            return Ok(data);
        }
        #endregion Users

        #region Login Api
        public AdminAPIController(ILoginServices iLoginServices)
        {
            _iLoginServices = iLoginServices;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("api/LoginApi/")]
        public async Task<IActionResult> Authenticate(LoginVM users)
        {
            var data = await _iLoginServices.Authenticate(users);
            return Ok(data);
        }


        [HttpPost]
        [Route("api/LogoutApi/")]
        public async Task<IActionResult> Logout()
        {
            var data = await _iLoginServices.Logout();
            return Ok(data);
        }
        #endregion Login Api

        #region Website 
        [HttpGet]

        [Route("api/WebsiteList/")]

        public async Task<IActionResult> GetWebsiteList()
        {
            var data = await _iLoginServices.GetWebsiteList();

            return Ok(data);
        }
        
        
        [AllowAnonymous]
        [HttpGet]
        [Route("api/GetUserCurrentWebsiteId/{userId}")]

        public async Task<IActionResult> GetUserCurrentWebsiteId(Guid userId)
        {
            var data = await _iLoginServices.GetUserCurrentWebsiteId(userId);

            return Ok(data);
        }


        [AllowAnonymous]
        [HttpPost]

        [Route("api/UpdateUserWebsite/")]

        public async Task<IActionResult> UpdateUserWebsite(UserWebsiteVM model)
        {
            var data = await _iLoginServices.UpdateUserWebsite( model);

            return Ok(data);
        }
        #endregion Website 
    }
}
