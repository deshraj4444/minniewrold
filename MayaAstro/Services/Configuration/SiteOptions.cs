namespace MayaAstro.Services.Configuration
{
    public class SiteOptions
    {
        public string SiteName { get; set; } = "MayaAstro";
        public string DefaultTitle { get; set; } = "MayaAstro - Daily Horoscope, Astrology Blogs and Spiritual Guidance";
        public string DefaultDescription { get; set; } = "Read daily horoscopes, astrology guidance, spiritual blogs, videos and expert astro services from MayaAstro.";
        public string DefaultKeywords { get; set; } = "daily horoscope, astrology, zodiac, kundli, astro services, spiritual blogs";
        public string PublicBaseUrl { get; set; } = "https://www.mayaastro.com";
        public string OgImage { get; set; } = "/img/logo.png";
        public string ThemeColor { get; set; } = "#f28c28";
        public List<WebsiteDomainOption> Websites { get; set; } = new();
    }

    public class WebsiteDomainOption
    {
        public string Host { get; set; }
        public int WebsiteId { get; set; } = 1;
        public string SiteName { get; set; }
        public string PublicBaseUrl { get; set; }
    }
}
