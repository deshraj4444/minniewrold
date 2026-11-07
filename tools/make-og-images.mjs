// Renders a 1200×630 social-share thumbnail (Facebook/WhatsApp link preview) for every seed test,
// plus one for the tests list, into client/public/og/tests/<slug>.png using headless Chrome or Edge.
// Usage: node tools/make-og-images.mjs            (all tests)
//        node tools/make-og-images.mjs <slug>...  (only these)
// Re-run after adding a seed file, then build the client (`npx vite build` in client/).
import { readFileSync, readdirSync, writeFileSync, mkdirSync, existsSync, rmSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { execFileSync } from 'node:child_process';
import { tmpdir } from 'node:os';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const seedDir = join(root, 'MinnieWorld.Web', 'Data', 'Seed', 'exams');
const outDir = join(root, 'client', 'public', 'og', 'tests');
const work = join(tmpdir(), 'mw-og');
mkdirSync(outDir, { recursive: true });
mkdirSync(work, { recursive: true });

const browser = [
  'C:/Program Files/Google/Chrome/Application/chrome.exe',
  'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
  '/usr/bin/google-chrome',
  '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome',
].find(existsSync);
if (!browser) throw new Error('Chrome or Edge is needed to render the images.');

const examHi = {
  'HP TET TGT Arts': 'हिमाचल प्रदेश TET • TGT कला',
  'HP TET JBT': 'हिमाचल प्रदेश TET • JBT',
  'CTET Paper I': 'केंद्रीय शिक्षक पात्रता परीक्षा • पेपर I',
  'CTET Paper II (Social Studies)': 'CTET पेपर II • सामाजिक अध्ययन',
};
const sectionInfo = {
  'Child Development & Pedagogy': ['बाल विकास एवं शिक्षाशास्त्र', '#6d3fc0'],
  'General Awareness': ['सामान्य जागरूकता', '#c2502e'],
  English: ['English', '#1e6fb8'],
  Hindi: ['हिंदी', '#b8336a'],
  Sanskrit: ['संस्कृत', '#8a5a00'],
  'Social Studies': ['सामाजिक अध्ययन', '#2f7d4f'],
  Mathematics: ['गणित', '#0f766e'],
  Science: ['विज्ञान', '#0369a1'],
  'Environmental Studies': ['पर्यावरण अध्ययन', '#4d7c0f'],
};
const kindLabel = {
  'Mock test': ['FULL MOCK TEST', 'फुल मॉक टेस्ट'],
  'Previous year': ['PREVIOUS YEAR PAPER', 'पिछले वर्ष का प्रश्नपत्र • उत्तर कुंजी सहित'],
  'Sample test': ['FREE TEST', 'निःशुल्क टेस्ट'],
  Sectional: ['SECTIONAL TEST', 'खंडवार टेस्ट'],
  'Year pattern': ['PRACTICE PAPER • YEAR PATTERN', 'वर्ष-पैटर्न प्रश्नपत्र • उत्तर कुंजी सहित'],
  'Expected paper': ['EXPECTED PAPER', 'संभावित प्रश्नपत्र • उत्तर कुंजी सहित'],
};
const esc = (s) => String(s).replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]);

function page({ exam, hi, badge, subtitle, subtitleHi, chips, sections, price }) {
  return `<!doctype html><html><head><meta charset="utf-8">
<link href="https://fonts.googleapis.com/css2?family=Poppins:wght@500;600;700;800&family=Hind:wght@500;600&display=block" rel="stylesheet">
<style>
*{box-sizing:border-box;margin:0}
html,body{width:1200px;height:630px;overflow:hidden}
body{font-family:Poppins,'Segoe UI',sans-serif;background:#17264a;color:#fff;position:relative}
[lang=hi],.hi{font-family:Hind,'Nirmala UI',sans-serif}
.glow{position:absolute;right:-160px;top:-160px;width:560px;height:560px;border-radius:50%;background:radial-gradient(circle,#e8821e55,transparent 65%)}
.top{display:flex;justify-content:space-between;align-items:center;padding:44px 64px 0}
.brand{font-weight:600;font-size:24px;opacity:.85;letter-spacing:.5px}
.badge{background:#e8821e;border-radius:99px;padding:10px 26px;font-weight:700;font-size:24px;letter-spacing:1px}
.main{padding:34px 64px 0}
h1{font-size:${exam.length > 22 ? 62 : 76}px;font-weight:800;line-height:1.05;letter-spacing:-1px}
.hi1{font-size:34px;color:#c9d3ea;margin-top:8px;font-weight:500}
.sub{margin-top:24px;font-size:36px;font-weight:600;color:#ffd7ad}
.sub small{display:block;font-size:26px;color:#c9d3ea;font-weight:500;margin-top:2px}
.chips{display:flex;gap:14px;margin-top:28px}
.chip{background:#ffffff1a;border:2px solid #ffffff33;border-radius:14px;padding:8px 18px;font-size:24px;font-weight:600}
.bottom{position:absolute;left:0;right:0;bottom:0;height:86px;background:#f8f6f1;display:flex;align-items:center;gap:10px;padding:0 64px;color:#17264a}
.sec{background:var(--c);color:#fff;border-radius:99px;padding:6px 16px;font-size:19px;font-weight:600;white-space:nowrap}
.price{margin-left:auto;font-weight:800;font-size:34px;color:${price === 'FREE' ? '#1f8a55' : '#e8821e'}}
</style></head><body>
<div class="glow"></div>
<div class="top"><span class="brand">Curious Minds · Online Mock Tests</span><span class="badge">${esc(badge)}</span></div>
<div class="main">
<h1>${esc(exam)}</h1>
${hi ? `<div class="hi1" lang="hi">${esc(hi)}</div>` : ''}
<div class="sub">${esc(subtitle)}<small lang="hi">${esc(subtitleHi)}</small></div>
<div class="chips">${chips.map((c) => `<span class="chip">${esc(c)}</span>`).join('')}</div>
</div>
<div class="bottom">${sections.slice(0, 5).map((s) => `<span class="sec hi" style="--c:${sectionInfo[s]?.[1] ?? '#475569'}">${esc(sectionInfo[s]?.[0] ?? s)}</span>`).join('')}<span class="price">${esc(price)}</span></div>
</body></html>`;
}

function render(name, html) {
  const file = join(work, `${name}.html`);
  writeFileSync(file, html);
  const out = join(outDir, `${name}.png`);
  execFileSync(browser, ['--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
    '--window-size=1200,630', '--virtual-time-budget=6000', `--screenshot=${out}`, pathToFileURL(file).href], { stdio: 'ignore' });
  console.log(`✓ ${name}.png`);
}

const only = process.argv.slice(2);
const exams = readdirSync(seedDir).filter((f) => f.endsWith('.json')).map((f) => JSON.parse(readFileSync(join(seedDir, f), 'utf8')));

for (const e of exams) {
  if (only.length && !only.includes(e.slug)) continue;
  const n = e.questions.length;
  const sections = [...new Set(e.questions.map((q) => q.section))];
  const [badge, kindHi] = kindLabel[e.kind] ?? [e.kind.toUpperCase(), ''];
  // "HP TET TGT Arts · November 2022 (previous year questions)" → "November 2022"
  let subtitle = (e.title.split('·')[1] ?? e.title).replace(/\(previous year questions\)/i, '').trim();
  if (e.kind === 'Previous year') subtitle = `${subtitle} Paper with Answer Key`;
  render(e.slug, page({
    exam: e.category,
    hi: examHi[e.category],
    badge: e.kind === 'Previous year' && e.year ? `${badge} ${e.year}` : badge,
    subtitle,
    subtitleHi: kindHi,
    chips: [`${n} Questions`, `${e.durationMinutes} Min`, 'English + हिंदी', 'Instant Result'],
    sections,
    price: e.price > 0 ? `₹${e.price}` : 'FREE',
  }));
}

if (!only.length || only.includes('index')) {
  render('index', page({
    exam: 'HP TET · CTET · JBT',
    hi: 'मॉक टेस्ट एवं पिछले वर्षों के प्रश्नपत्र',
    badge: 'MOCK TESTS',
    subtitle: 'Mock Tests & Previous Year Papers',
    subtitleHi: 'उत्तर कुंजी, रैंक और तुरंत परिणाम के साथ',
    chips: [`${exams.length}+ Tests`, 'English + हिंदी', 'Answer Keys', 'Free Tests'],
    sections: ['Child Development & Pedagogy', 'General Awareness', 'Social Studies', 'Mathematics', 'English'],
    price: 'FREE',
  }));
}
rmSync(work, { recursive: true, force: true });
