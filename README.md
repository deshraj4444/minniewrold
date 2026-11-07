# MinnieWorld

One website, one domain, one database, holding several sites with completely different designs:

| URL | Site | What it is |
|---|---|---|
| `/` | **MinnieWorld hub** | Thumbnail tiles linking to each site (managed in admin → *Hub tiles*) |
| `/astro` | **AstroAura** | Vedic astrology portal in the style of AstroSage: free Kundali, Kundli Milan, Panchang, horoscopes, numerology, consultation requests |
| `/dharma` | **Dharma Path** | Religious blog: articles plus learning series (courses with lessons) |
| `/gk` | **Curious Minds** | Exam prep and general knowledge: articles, courses, and timed **mock tests** with student accounts and instant results (`/gk/tests`). Entry points: *Below Class 6* (Class 6 admission tests), *CBSE Class 6*, *IIT JEE* and *Teachers (TET)* |
| `/jee` | **Curious Minds: IIT JEE** | Free JEE chapter tests (+4 / −1) with every solution on the page, and chapter notes |
| `/cbse/class-6` | **Curious Minds: CBSE Class 6** | Mathematics, English and Personality Development: notes, learning games and practice questions. No login |
| `/admin` | **Admin** | Manage posts, courses, lessons, mock tests and questions, students, consultations, messages, testimonials, hub tiles, settings (WhatsApp number) |

## Why one site with paths (not subdomains)

- One domain and one SSL certificate cover everything (`yourdomain.com/astro`, `/dharma`, `/gk`).
- One deployment and one database to back up. Each site owns its own SQL schema (`core`, `astro`, `blog`), so a site can move to its own database later without rewriting it.
- Search ranking accrues to one domain.
- Adding another site later means adding a folder in `client/src/`, a route in `App.jsx`, and a tile in admin.

Subdomains (`astro.yourdomain.com`) are also free with a single domain if you ever want them. They need only a DNS record and a host binding, and the same app can serve them.

## Project layout

```
MinnieWorld.Web/        ASP.NET Core (.NET 10) API, serves the built React app
  Endpoints/            Public, auth, admin, exam, CBSE and SEO (robots.txt, sitemap.xml, 301s) endpoints
  Services/Astrology/   Ephemeris, Kundali, Panchang, Matching, Numerology (no paid APIs)
  Services/Seo/         The /jee page catalog (JeePages.cs)
  Services/Cbse/        CBSE chapter model, validation and catalog
  Services/SpaHost*.cs  Serves the React app with per-page head tags, JSON-LD and server-rendered /jee and /cbse content
  Data/                 EF Core DbContext, migrations, seeder, Seed/*.json starter content, Seed/cbse chapter files
client/                 React 19 + Vite. One bundle per site.
  src/hub  src/astro  src/dharma  src/gk  src/admin
  src/gk/games/         The six learning games (data-driven, used by CBSE chapters)
  src/gk/cbse/          CBSE pages
  public/covers/        Cover images used by the starter content
MinnieWorld.Tests/      Astrology engine tests, CBSE validation and SEO redirect tests
```

## Run locally

Requirements: .NET 10 SDK, Node 22+, SQL Server (Express is fine).

1. Check the connection string in `MinnieWorld.Web/appsettings.Development.json` (default: `localhost\SQLEXPRESS`, Windows auth).
2. Start the API. It creates the database, applies migrations and seeds starter content on first run.
   ```bash
   cd MinnieWorld.Web
   dotnet run --launch-profile http        # http://localhost:5292
   ```
3. Start the React dev server in another terminal:
   ```bash
   cd client
   npm install
   npm run dev                             # http://localhost:5173 (proxies /api to :5292)
   ```
4. Admin: `http://localhost:5173/admin`. The development login is in `appsettings.Development.json` (`Admin:Email` / `Admin:Password`).

Run the tests with `dotnet test MinnieWorld.Tests`.

## Deploy

```bash
dotnet publish MinnieWorld.Web -c Release -o publish
```

`publish` runs `npm ci && npm run build` and puts the React app in `wwwroot`, so the `publish` folder is the whole website. Host it on IIS, Azure App Service or Linux behind nginx.

Set these in production with environment variables (or your host's settings). **Never commit real secrets.**

| Setting | Example |
|---|---|
| `ConnectionStrings__MinnieWorldDb` | your SQL Server connection string |
| `Admin__Email`, `Admin__Password` | first admin account (only used while no admin exists) |
| `Site__PublicUrl` | `https://yourdomain.com` (used for share links and preview images) |

Uploaded images are stored in `App_Data/uploads` next to the app. Include that folder in your backups.

## Content

- **Posts and courses**: admin → Posts / Courses. Content is Markdown, with a live preview. Images can be uploaded.
- **Starter content** comes from `MinnieWorld.Web/Data/Seed/dharma.json` and `gk.json`. It's loaded only into an empty blog, so it never overwrites your edits.
- **Content library**: `Data/Seed/posts/{blog}/*.json` (`{ "posts": [{ "slug", "title", "excerpt", "content", "category", "tags" }] }`). Any post whose slug isn't in that blog yet is added on start-up, so new files can be dropped in at any time. Dharma Path ships with 28 Vedic/Puranic mantras, 34 stotras and aartis (full Sanskrit text from sanskritdocuments.org / Wikisource with Hindi meanings) and 100 temple-history articles, all in Hindi.
- **Sharing**: every post, course and lesson has Facebook, WhatsApp, X, Telegram, LinkedIn, Instagram and copy-link buttons. The server writes each page's title, description and image into the HTML, so links shared on Facebook and WhatsApp show a proper preview card. Instagram doesn't allow web share links: on phones the Instagram button opens the phone's share sheet, and on desktop it copies the link for your bio or Story.

## IIT JEE (`/jee`)

- Pages are listed in `MinnieWorld.Web/Services/Seo/JeePages.cs` and mirrored in `client/src/gk/jee.js`; keep the two in step. A page is a test (an exam seed file) or notes (a post seed file).
- Old `/gk/...` URLs of these pages answer with a 301 to the `/jee` URL.
- Every question, option and solution of a JEE chapter test is in the page's HTML (inside collapsed "Answer and solution" boxes), so search engines index it.

## CBSE (`/cbse/class-6`)

- Only Class 6 for now, with three subjects: Mathematics, English and Personality Development (`CbseCatalog.Classes` / `Subjects`). `/cbse` redirects to `/cbse/class-6`.
- **One JSON file per chapter**: `Data/Seed/cbse/class-6/{subject}/{NN}-{slug}.json`, served at `/cbse/class-6/{subject}/{slug}`. The format is documented as JSON Schema in `Data/Seed/cbse/schema/` (chapter + the six game types: dragSort, matchPairs, timeline, labelDiagram, numberLine, speedQuiz). Give both schema files to an AI to draft a new chapter.
- Files are checked on load. A file with a problem is skipped and the reason is written to the log, so one bad file never breaks the section. Changes show on refresh in Development and after a restart in production.
- Pages with no chapters yet say "coming soon" and are `noindex`, and are left out of the sitemap.

## SEO

- `robots.txt` and `sitemap.xml` are generated by the app (`Endpoints/SeoEndpoints.cs`). The sitemap lists the hubs plus every `/jee` page and CBSE chapter, with `lastmod`.
- Each `/jee` and `/cbse` page gets its own title, description, canonical URL, Open Graph and Twitter tags, and JSON-LD. The content is also rendered on the server, so it is in the HTML without JavaScript. Check it with `curl -s https://yourdomain.com/<page> | grep "<h1>"`.

## Mock tests

- Students register at `/gk/register` (separate from admin accounts) and take timed tests at `/gk/tests`. The clock runs on the server, answers autosave, and the test submits itself when time runs out.
- Results show score, pass/fail (60%), rank and percentile among all attempts, section-wise analysis, and every question with the correct answer and explanation.
- **Sections by audience:** `/gk/tests` groups tests by audience and then by exam. *Class 6 admission* (`/gk/class-6`) covers JNVST, RMS CET and Sainik School AISSEE. *Teacher eligibility* (`/gk/teachers`) covers HP TET and CTET. Each exam has its own page (`/gk/exams/<key>`) with its papers grouped by type. The grouping lives in `client/src/gk/exams.js`, which maps a test's *category* to an exam. A new category shows under "More tests" until it is added there.
- **Class 6 papers:** each exam has 5 *Year pattern* papers (original papers on each of the last 5 years' patterns, not copies of the official papers) and 5 *Expected papers*, all free. Picture (non-verbal) questions carry an SVG `figure`. Reading questions carry a `passage`. JNVST questions are worth 1.25 marks each and AISSEE questions 2 or 3 marks, set per question with `marks`.
- **Certificates:** passing a test (60%+) earns a printable certificate at `/gk/certificate/<code>`: Bronze from 60%, Silver from 75% and Gold from 90%. The page is public so it can be shared, and the server checks the code, so made-up codes show "not found". Students see their certificates under *My results*.
- **Quiz game:** `/gk/play/<exam key>` serves 10 random questions from that exam's free papers. Players get 3 lives, 60 seconds per question, streak bonuses and stars at the end. No login is needed.
- **Adding tests:** admin → *Tests & questions*. Add questions one at a time, or **Import JSON**: an array of `{ "question", "options": [4], "answer": "B" | option text | 0–3, "explanation", "section" }`. Other common field names (`text`, `q`, `option_a`…, `correct`, `subject`) are understood too. Optional: `marks`, `figure` (SVG), `passage`/`passageHi`.
- **Checking seed files:** `node tools/validate-exam.mjs` validates every seed file. `node tools/render-figures.mjs <file.json> <outDir>` renders each SVG figure to a PNG, so you can check the pictures by eye.
- **Starter tests** come from `MinnieWorld.Web/Data/Seed/exams/*.json` (same format plus `slug`, `title`, `category`, `kind`, `durationMinutes`). A file whose slug isn't in the database yet is loaded on the next start; existing tests are never overwritten. Included: three original 150-question HP TET TGT (Arts) mock tests and previous-year questions from the June and November 2021 papers.

## Students and access

Admin → *Students*: add students yourself (a random password is generated, with buttons to copy the login details or send them on WhatsApp), edit them, reset passwords, disable or delete accounts. Give an **All-Access pass** (optionally until a date) to unlock every paid test, or unlock single tests without a payment. Resetting a password or disabling an account signs the student out everywhere.

Free tests: the five *Sample test* papers (a 30-question mini mock and four 20-question sectional tests). Full mocks and previous-year papers cost ₹10 each.

## Paid tests (UPI, approved by hand)

Each test has a price (0 = free). There's no payment gateway: the money goes to the owner's personal UPI ID.

1. Set your UPI ID and name in admin → *Settings*.
2. A student opens a paid test, scans the QR code (or taps the UPI app button on a phone) and pays. The note `MW<student>T<test>` is filled in for them.
3. They enter the 12-digit UPI transaction ID from their receipt. The test shows "waiting for confirmation" and checks back every 20 seconds.
4. In admin → *Payments*, compare the ID with your Google Pay history and **Approve** (the test unlocks for that student, with unlimited retakes) or **Reject** with a reason (they can resubmit).

A transaction ID can only unlock one test. To switch to automatic confirmation later, add a payment gateway (e.g. Razorpay or Cashfree, which accept individuals with PAN) and approve purchases from its webhook instead.

## WhatsApp chat with the astrologer

Set the astrologer's WhatsApp number in admin → *Settings*. Every AstroAura page then shows a "Chat on WhatsApp" button, and messages go straight to that WhatsApp. Leave it empty to hide the buttons.

## Rate us on Google

Paste the Google Business Profile review link (e.g. `https://g.page/r/…/review`) in admin → *Settings*. The Curious Minds footer and result pages then show a "Rate us on Google" button. Only Google links are accepted, and the button is hidden while the field is empty.

## Astrology engine

All calculations run on the server with no external service:

- Planet positions use Meeus (Sun, Moon) and JPL Keplerian elements (planets). Ayanamsa is Lahiri, nodes are mean nodes, houses are whole-sign.
- **Kundali**: Lagna, 9 grahas (sign, degree, nakshatra, pada, house, retrograde, dignity), Lagna, Moon and Navamsa charts (North Indian style), Avakhada details, Vimshottari mahadasha and antardasha, Manglik check.
- **Kundli Milan**: Ashtakoota, all 8 kootas (36 gunas), Nadi and Bhakoot dosha, Manglik compatibility.
- **Yogas & doshas**: Gajakesari, Budhaditya, Chandra-Mangal, Pancha Mahapurusha, Sunapha/Anapha/Durudhara/Kemadruma; Manglik and Kalsarp. **Sade Sati** (current phase and every period over 100 years), Shani Dhaiya, **D10** chart and current **transits** from the Moon.
- **Varshphal** (Tajik annual chart): exact solar return, Varsha Lagna, Muntha, the five office-bearers and year lord (strength by sign dignity, a simplified Pancha-vargiya bala), Punya Saham and Mudda dasha.
- **Panchang**: tithi, nakshatra, yoga and karana with end times; sunrise and sunset; Rahu Kaal, Yamaganda, Gulika, Abhijit and Brahma Muhurat; amanta and purnimanta month, Vikram and Shaka Samvat, ritu.
- Birth places come from Open-Meteo's free geocoding API, which also supplies the time zone.
- Daily, weekly and monthly horoscope text comes from a free horoscope API, cached in the database. A built-in fallback is used if that API is down.

Accuracy is checked in `MinnieWorld.Tests`:

- Meeus worked examples for the Moon, Sun and Venus.
- Sharad Purnima and Diwali 2024 tithi end times.
- Delhi sunrise and sunset within 3 minutes.
