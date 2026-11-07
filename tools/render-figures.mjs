// Renders every "figure" (SVG) in a seed test to PNG so it can be checked by eye before committing.
// Usage: node tools/render-figures.mjs <seed-file.json> [outDir] [fromQ] [toQ]
// Writes <outDir>/q<N>.png (one per figure question, 1-based question numbers) using headless Chrome or Edge.
// Default outDir: <os tmp>/mw-figures/<slug>
import { readFileSync, writeFileSync, mkdirSync, existsSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { execFileSync } from 'node:child_process';
import { tmpdir } from 'node:os';

const [file, outArg, fromArg, toArg] = process.argv.slice(2);
if (!file) { console.log('Usage: node tools/render-figures.mjs <seed-file.json> [outDir] [fromQ] [toQ]'); process.exit(1); }
const d = JSON.parse(readFileSync(file, 'utf8'));
const outDir = resolve(outArg || join(tmpdir(), 'mw-figures', d.slug ?? 'test')); // Chrome needs an absolute path
mkdirSync(outDir, { recursive: true });
const browser = [
  'C:/Program Files/Google/Chrome/Application/chrome.exe',
  'C:/Program Files (x86)/Google/Chrome/Application/chrome.exe',
  'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
  'C:/Program Files/Microsoft/Edge/Application/msedge.exe',
  '/usr/bin/google-chrome', '/usr/bin/chromium'
].find(existsSync);
if (!browser) { console.log('Chrome or Edge not found'); process.exit(1); }

const from = Number(fromArg) || 1;
const to = Number(toArg) || Infinity;
const esc = (s) => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;');
let n = 0;
(d.questions ?? []).forEach((q, i) => {
  const no = i + 1;
  if (!q.figure || no < from || no > to) return;
  // Shown exactly as the site shows it: as an <img> from a data URI, on a white card 640px wide.
  const src = `data:image/svg+xml;charset=utf-8,${encodeURIComponent(q.figure)}`;
  const html = `<!doctype html><meta charset="utf-8"><body style="margin:0;padding:12px;background:#fff;font:15px Arial;width:616px">
<b>Q${no}.</b> ${esc(q.text)}<br><img src="${src}" style="max-width:100%;display:block;margin:10px 0;border:1px solid #ddd">
<small>Answer: ${'ABCD'[q.answer]}</small></body>`;
  const page = join(outDir, `q${no}.html`);
  writeFileSync(page, html);
  const out = join(outDir, `q${no}.png`);
  execFileSync(browser, ['--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
    '--window-size=640,560', '--virtual-time-budget=3000', `--screenshot=${out}`, pathToFileURL(page).href], { stdio: 'ignore' });
  n++;
});
console.log(`Rendered ${n} figure(s) to ${outDir}`);
