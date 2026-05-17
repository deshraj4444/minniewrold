using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using ImageMagick;
namespace MayaAstro.Controllers
{
    [Route("CropImage")]
    [ApiController]
    public class CropImageController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        
        private readonly string _baseUrl;
        public CropImageController(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _baseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        [HttpPost("UploadCKImage")]
        public async Task<IActionResult> UploadCKImage(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
                return BadRequest(new { error = new { message = "No file uploaded" } });

            try
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string baseName = "image";
                string shortGuid = Guid.NewGuid().ToString("N").Substring(0, 4);
                string fileName = $"{baseName}{shortGuid}.webp";
                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var memoryStream = new MemoryStream())
                {
                    await upload.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (var magickImage = new MagickImage(memoryStream))
                    {
                        await using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            magickImage.Format = MagickFormat.WebP;
                            await magickImage.WriteAsync(fileStream);
                        }
                    }
                }

               
                string url = $"{_baseUrl}/uploads/{fileName}";
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = new { message = ex.Message } });
            }
        }


    }
}
