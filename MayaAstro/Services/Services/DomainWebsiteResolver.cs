using MayaAstro.Services.Configuration;
using Microsoft.Extensions.Options;

namespace MayaAstro.Services.Services
{
    public interface IDomainWebsiteResolver
    {
        int GetCurrentWebsiteId();
        string GetSiteName();
        string GetPublicBaseUrl();
    }

    public class DomainWebsiteResolver : IDomainWebsiteResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SiteOptions _siteOptions;

        public DomainWebsiteResolver(IHttpContextAccessor httpContextAccessor, IOptions<SiteOptions> siteOptions)
        {
            _httpContextAccessor = httpContextAccessor;
            _siteOptions = siteOptions.Value;
        }

        public int GetCurrentWebsiteId()
        {
            return GetCurrentWebsite()?.WebsiteId ?? 1;
        }

        public string GetSiteName()
        {
            return GetCurrentWebsite()?.SiteName ?? _siteOptions.SiteName;
        }

        public string GetPublicBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var configuredUrl = GetCurrentWebsite()?.PublicBaseUrl ?? _siteOptions.PublicBaseUrl;

            if (!string.IsNullOrWhiteSpace(configuredUrl))
            {
                return configuredUrl.TrimEnd('/');
            }

            if (request == null)
            {
                return string.Empty;
            }

            return $"{request.Scheme}://{request.Host}".TrimEnd('/');
        }

        private WebsiteDomainOption GetCurrentWebsite()
        {
            var host = _httpContextAccessor.HttpContext?.Request?.Host.Host;
            if (string.IsNullOrWhiteSpace(host))
            {
                return _siteOptions.Websites.FirstOrDefault();
            }

            return _siteOptions.Websites.FirstOrDefault(website =>
                !string.IsNullOrWhiteSpace(website.Host) &&
                string.Equals(website.Host.Trim(), host.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
