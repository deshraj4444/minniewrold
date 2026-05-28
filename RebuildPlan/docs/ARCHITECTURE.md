# Fresh Website Rebuild (Clean Architecture)

## Stack
- **Backend:** ASP.NET Core 8 Web API + MVC SSR for SEO pages.
- **Frontend:** React 18 + Vite (public site + admin UI).
- **Database:** SQL Server.
- **Auth:** Cookie auth for admin panel.

## Public Pages (SSR/SEO)
- Home
- About
- Blog list
- Blog detail
- Quotes list
- Quote detail
- Videos list
- Video detail
- Services
- Acharya
- Contact
- Privacy Policy
- Terms of Use

## Admin Scope (only requested modules)
- Blog management (CRUD)
- Quote management (CRUD)
- YouTube links management (CRUD)
- Website-wide SEO settings

## SEO Features
- Canonical, Open Graph, Twitter cards
- JSON-LD (`WebSite`, `Article`, `VideoObject`)
- Robots + sitemap
- Clean slug routing

## Project Layout
- `src/backend` -> ASP.NET Core API + SSR endpoints for SEO pages.
- `src/frontend` -> React app (public + admin).
- `database` -> schema + seed scripts.

## Notes
- Old modules (horoscope engines, products, jeweller, banners, etc.) are intentionally excluded.
- New model is normalized around content + SEO only.
