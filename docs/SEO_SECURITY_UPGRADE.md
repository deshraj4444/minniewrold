# SEO, Security, and React Migration Notes

This upgrade does **not** require an immediate database schema change. The application now generates safer metadata, `/robots.txt`, and `/sitemap.xml` from existing content fields.

## Recommended configuration for multi-domain content

Add host-to-website mappings in production configuration so the public website automatically fetches blogs/videos for the correct admin website ID:

```json
{
  "DefaultWebsiteId": 1,
  "WebsiteDomainMappings": {
    "mayaastro.com": 1,
    "www.mayaastro.com": 1,
    "mayait.example.com": 2
  }
}
```

## Optional database hardening script

Run this only after confirming the exact SQL Server table names in production. It adds lookup indexes for published content, URL slugs, and website/category filtering; it does not change data shape.

```sql
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_blog_detail_seo_lookup' AND object_id = OBJECT_ID('dbo.blog_detail')
)
BEGIN
    CREATE INDEX IX_blog_detail_seo_lookup
    ON dbo.blog_detail (is_published, is_deleted, website_id, type_id, page_url)
    INCLUDE (modified_date, publish_date, blog_category_id);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_blog_category_website' AND object_id = OBJECT_ID('dbo.blog_category')
)
BEGIN
    CREATE INDEX IX_blog_category_website
    ON dbo.blog_category (website_id, is_active, blog_category_name);
END;
```

## Future React migration plan

For Google-friendly SEO, do **not** ship a client-only React single page app for public pages. The recommended path is:

1. Keep ASP.NET Core as the API/admin backend.
2. Move public pages to React with server-side rendering or static generation (Next.js or Astro + React islands).
3. Keep high-value SEO pages server-rendered: home, blog detail, video detail, category pages, zodiac/daily horoscope pages.
4. Continue to expose admin APIs from ASP.NET Core and share DTOs/contracts for blogs, videos, quotes, websites, and horoscope modules.
5. Add structured data per content type (`Article`, `VideoObject`, `FAQPage`, `BreadcrumbList`) as each page is migrated.
