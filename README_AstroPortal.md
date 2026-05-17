# AstroAura React + .NET Core + MSSQL Website

This repository now includes a new greenfield astrology website stack alongside the legacy MVC app:

- `astro-frontend/` — React + Vite single-page frontend.
- `AstroPortal.Api/` — .NET 8 minimal API backend.
- `AstroPortal.Api/Data/Seed.sql` — MSSQL schema and dummy seed data script.

## Features

- Daily horoscope page backed by a free horoscope API and cached in MSSQL.
- Blog listing and simple admin-side blog creation/update/delete endpoints.
- Kundali horoscope service request form.
- Testimonials page powered by database data.
- Contact us form saved to MSSQL.
- Product page showing religious/spiritual items.
- Dummy MSSQL seed data for blogs, products and testimonials.

## Run the API

Install the .NET 8 SDK, ensure SQL Server is running, then update `AstroPortal.Api/appsettings.Development.json` if your SQL Server connection string differs.

```bash
cd AstroPortal.Api
dotnet restore
dotnet run
```

The API seeds the database on startup using `DbInitializer.SeedAsync`. You can also run `Data/Seed.sql` manually in SQL Server Management Studio or Azure Data Studio.

## Run the React frontend

```bash
cd astro-frontend
npm install
npm run dev
```

If your API is not on `http://localhost:5000/api`, create `astro-frontend/.env`:

```bash
VITE_API_BASE_URL=https://localhost:7001/api
```

## Free horoscope API

The backend is configured to call `https://freehoroscopeapi.com/api/v1/get-horoscope/daily?sign=aries` by default. If that service is unavailable, the API returns a local fallback horoscope and still caches the response shape.

## Production notes

- Add authentication/authorization around `/api/admin/*` before production.
- Replace Unsplash demo images with owned or licensed assets.
- Use EF Core migrations for production schema management instead of `EnsureCreated`.
