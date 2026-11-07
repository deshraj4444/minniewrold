// Checks seed test files in MinnieWorld.Web/Data/Seed/exams before they are committed.
// Usage: node tools/validate-exam.mjs [file.json ...]   (no args = every file in the folder)
import { readFileSync, readdirSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const folder = join(dirname(fileURLToPath(import.meta.url)), '..', 'MinnieWorld.Web', 'Data', 'Seed', 'exams');
const files = process.argv.slice(2).length ? process.argv.slice(2) : readdirSync(folder).filter((f) => f.endsWith('.json')).map((f) => join(folder, f));
const kinds = ['Mock test', 'Previous year', 'Sectional', 'Sample test', 'Year pattern', 'Expected paper'];
const deva = /[ऀ-ॿ]/;
let failed = 0;
const slugs = new Map();

for (const file of files) {
  const errs = [];
  const warn = [];
  let d;
  try { d = JSON.parse(readFileSync(file, 'utf8')); } catch (e) { console.log(`✗ ${file}: invalid JSON: ${e.message}`); failed++; continue; }
  for (const k of ['slug', 'title', 'category', 'kind', 'durationMinutes']) if (!d[k]) errs.push(`missing ${k}`);
  if (d.kind && !kinds.includes(d.kind)) errs.push(`kind must be one of ${kinds.join(', ')}`);
  if ((d.kind === 'Previous year' || d.kind === 'Year pattern') && !d.year) errs.push(`${d.kind} paper needs "year"`);
  if (slugs.has(d.slug)) errs.push(`slug ${d.slug} also used by ${slugs.get(d.slug)}`);
  slugs.set(d.slug, file);
  const qs = d.questions ?? [];
  if (!qs.length) errs.push('no questions');
  const seen = new Set();
  let hi = 0;
  qs.forEach((q, i) => {
    const n = `Q${i + 1}`;
    if (!q.section) errs.push(`${n}: missing section`);
    if (!q.text?.trim()) errs.push(`${n}: missing text`);
    if (!Array.isArray(q.options) || q.options.length !== 4 || q.options.some((o) => !String(o).trim())) errs.push(`${n}: needs 4 non-empty options`);
    if (!Number.isInteger(q.answer) || q.answer < 0 || q.answer > 3) errs.push(`${n}: answer must be an integer 0-3`);
    if (q.optionsHi != null && (!Array.isArray(q.optionsHi) || q.optionsHi.length !== 4)) errs.push(`${n}: optionsHi must be 4 items`);
    if (q.textHi) {
      hi++;
      if (!deva.test(q.textHi)) errs.push(`${n}: textHi has no Devanagari`);
      if (!q.optionsHi) warn.push(`${n}: has textHi but no optionsHi`);
    }
    if (deva.test(q.text) && q.section !== 'Hindi' && q.section !== 'Sanskrit') warn.push(`${n}: English "text" contains Devanagari (put Hindi in textHi)`);
    if (q.figure != null) {
      const f = String(q.figure);
      if (!/^\s*<svg[\s>]/.test(f) || !/<\/svg>\s*$/.test(f)) errs.push(`${n}: figure must be one <svg>…</svg> element`);
      if (!/viewBox=/.test(f)) errs.push(`${n}: figure needs a viewBox`);
      if (!f.includes('xmlns="http://www.w3.org/2000/svg"')) errs.push(`${n}: figure needs xmlns="http://www.w3.org/2000/svg"`);
      if (/<script|<foreignObject|\son\w+\s*=|href\s*=\s*["'](?!#)/i.test(f)) errs.push(`${n}: figure has scripts, event handlers, foreignObject or external links`);
      if (f.length > 12000) errs.push(`${n}: figure is ${f.length} chars (max 12000)`);
      try { decodeURIComponent(encodeURIComponent(f)); } catch { errs.push(`${n}: figure has invalid characters`); }
    }
    if (q.marks != null && !(typeof q.marks === 'number' && q.marks > 0 && q.marks <= 10)) errs.push(`${n}: marks must be a number from 0 to 10`);
    if (q.passageHi && !q.passage) errs.push(`${n}: passageHi without passage`);
    const key = (q.text?.trim().toLowerCase() ?? '') + (q.figure ? `#${i}` : '');
    if (seen.has(key)) warn.push(`${n}: duplicate question text`);
    seen.add(key);
  });
  const counts = qs.reduce((m, q) => ((m[q.section] = (m[q.section] ?? 0) + 1), m), {});
  const marks = qs.reduce((m, q) => m + (q.marks ?? 1), 0);
  const figs = qs.filter((q) => q.figure).length;
  const answers = [0, 1, 2, 3].map((a) => qs.filter((q) => q.answer === a).length);
  if (errs.length) failed++;
  console.log(`${errs.length ? '✗' : '✓'} ${file.split(/[\\/]/).pop()}: ${qs.length} q, ${marks} marks, ${figs} figures, ${hi} with Hindi, answers A/B/C/D=${answers.join('/')}, sections ${JSON.stringify(counts)}`);
  errs.slice(0, 30).forEach((e) => console.log(`   error: ${e}`));
  warn.slice(0, 10).forEach((e) => console.log(`   warn: ${e}`));
}
process.exit(failed ? 1 : 0);
