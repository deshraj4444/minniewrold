using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MayaAstro.Services.Services
{
    public class YouTubeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "AIzaSyAIY0-HEjQMqQ1U7GFRrBb5qaGw8L9lA6A"; 
        private readonly string _baseUrl = "https://www.googleapis.com/youtube/v3/search";

        public YouTubeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<YouTubeVideos>> GetVideosAsync(string channelId, int maxResults = 20)
        {
            var url = $"{_baseUrl}?part=snippet&channelId={channelId}&maxResults={maxResults}&order=date&type=video&key={_apiKey}";

            var response = await _httpClient.GetStringAsync(url);
            var jsonDoc = JsonDocument.Parse(response);
            var videos = new List<YouTubeVideos>();

            foreach (var item in jsonDoc.RootElement.GetProperty("items").EnumerateArray())
            {
                var video = new YouTubeVideos
                {
                    Title = item.GetProperty("snippet").GetProperty("title").GetString(),
                    VideoId = item.GetProperty("id").GetProperty("videoId").GetString(),
                    ThumbnailUrl = item.GetProperty("snippet").GetProperty("thumbnails").GetProperty("medium").GetProperty("url").GetString()
                };
                videos.Add(video);
            }

            return videos;
        }
    }

    public class YouTubeVideos
    {
        public string Title { get; set; }
        public string VideoId { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
