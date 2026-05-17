using MayaAstro.Services.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MayaAstro.Controllers
{
    public class YouTubeController : Controller
    {
        private readonly YouTubeService _youTubeService;

        public YouTubeController(YouTubeService youTubeService)
        {
            _youTubeService = youTubeService;
        }


        [Route("youtubeblog")]
        public async Task<IActionResult> Index(string channelId = "UCTBthH2uHarzjgxpgc4ICgw", int maxResults = 20)
        {
            var videos = await _youTubeService.GetVideosAsync(channelId, maxResults);
            return View(videos);
        }
    }
}
