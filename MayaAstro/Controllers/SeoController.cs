using System.Text;
using System.Xml.Linq;
using MayaAstro.Models;
using MayaAstro.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MayaAstro.Controllers
{
    public class SeoController : Controller
    {
        private static readonly string[] StaticRoutes =
        {
            "/",
            "/about",
            "/blog",
            "/videos",
            "/services",
            "/acharya",
            "/astrologers",
            "/appointments",
            "/contact",
            "/privacy-policy",
            "/term-of-use"
        };

        private static readonly string[] ZodiacSigns =
        {
            "aries", "taurus", "gemini", "cancer", "leo", "virgo",
            "libra", "scorpio", "sagittarius", "capricorn", "aquarius", "pisces"
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDomainWebsiteResolver _domainWebsiteResolver;
        private readonly ILogger<SeoController> _logger;

        public SeoController(
            IHttpClientFactory httpClientFactory,
            IDomainWebsiteResolver domainWebsiteResolver,
            ILogger<SeoController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _domainWebsiteResolver = domainWebsiteResolver;
            _logger = logger;
        }

        [HttpGet("robots.txt")]
        public ContentResult Robots()
        {
            var baseUrl = _domainWebsiteResolver.GetPublicBaseUrl();
            var robots = $"User-agent: *\nAllow: /\nDisallow: /Admin\nDisallow: /Jeweller\nDisallow: /api/\nSitemap: {baseUrl}/sitemap.xml\n";
            return Content(robots, "text/plain", Encoding.UTF8);
        }

        [HttpGet("sitemap.xml")]
        public async Task<ContentResult> Sitemap()
        {
            var baseUrl = _domainWebsiteResolver.GetPublicBaseUrl();
            var domainId = _domainWebsiteResolver.GetCurrentWebsiteId();
            var urls = StaticRoutes
                .Select(route => CreateUrl(baseUrl, route, DateTime.UtcNow, "weekly", route == "/" ? "1.0" : "0.8"))
                .Concat(ZodiacSigns.Select(sign => CreateUrl(baseUrl, $"/zodiac/{sign}", DateTime.UtcNow, "daily", "0.7")))
                .ToList();

            urls.AddRange(await GetBlogUrls(baseUrl, domainId));

            var document = new XDocument(
                new XElement(XName.Get("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9"), urls));

            return Content(document.ToString(), "application/xml", Encoding.UTF8);
        }

        private async Task<IEnumerable<XElement>> GetBlogUrls(string baseUrl, int domainId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MayaAstroApi");
                var responseMessage = await client.GetAsync($"api/HomeBlogList/{domainId}");
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return Enumerable.Empty<XElement>();
                }

                var responseData = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                if (response?.Data == null)
                {
                    return Enumerable.Empty<XElement>();
                }

                var blogs = JsonConvert.DeserializeObject<List<BlogDetailVM>>(response.Data.ToString()) ?? new List<BlogDetailVM>();
                return blogs
                    .Where(blog => !string.IsNullOrWhiteSpace(blog.PageUrl))
                    .Select(blog => CreateUrl(baseUrl, $"/{blog.PageUrl.TrimStart('/')}", blog.ModifiedDate == default ? blog.PublishDate : blog.ModifiedDate, "weekly", "0.9"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to build dynamic blog sitemap URLs.");
                return Enumerable.Empty<XElement>();
            }
        }

        private static XElement CreateUrl(string baseUrl, string route, DateTime? lastModified, string changeFrequency, string priority)
        {
            return new XElement(XName.Get("url", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                new XElement(XName.Get("loc", "http://www.sitemaps.org/schemas/sitemap/0.9"), $"{baseUrl}{route}"),
                new XElement(XName.Get("lastmod", "http://www.sitemaps.org/schemas/sitemap/0.9"), (lastModified ?? DateTime.UtcNow).ToString("yyyy-MM-dd")),
                new XElement(XName.Get("changefreq", "http://www.sitemaps.org/schemas/sitemap/0.9"), changeFrequency),
                new XElement(XName.Get("priority", "http://www.sitemaps.org/schemas/sitemap/0.9"), priority));
        }
    }
}
