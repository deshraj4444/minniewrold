import { useRef, useState } from 'react';
import { playCorrect, playWrong } from '../../lib/sound.js';
import { Bi, Feedback, GameFrame, plain } from './common.jsx';

const gcd = (a, b) => (b ? gcd(b, a % b) : Math.abs(a));
const decimals = (step) => (String(step).split('.')[1] ?? '').length;

/** Shows a value as the game asks: an integer, a decimal, or a fraction in lowest terms (−3/4, 5/2, 2). */
export function formatValue(v, format, step) {
  if (format === 'fraction') {
    const den = Math.round(1 / Math.min(step, 1));
    const num = Math.round(v * den);
    if (num % den === 0) return String(num / den).replace('-', '−');
    const g = gcd(num, den);
    return `${num < 0 ? '−' : ''}${Math.abs(num / g)}/${den / g}`;
  }
  return (format === 'decimal' ? v.toFixed(decimals(step)) : String(Math.round(v))).replace('-', '−');
}

const W = 1000;
const PAD = 40;

/**
 * Place a value on a number line. Tap or drag on the line, or use the arrow keys (one step), Page Up/Down (five
 * steps) and Home/End, then Check. A wrong answer shows where the value really is.
 */
export default function NumberLine({ game, lang }) {
  const { min, max, step } = game;
  const format = game.format ?? 'integer';
  const labelEvery = game.labelEvery ?? 1;
  const tolerance = game.tolerance ?? step / 2;
  const snap = (v) => Math.min(max, Math.max(min, Math.round((v - min) / step) * step + min));
  const middle = snap((min + max) / 2);

  const [index, setIndex] = useState(0);
  const [value, setValue] = useState(middle);
  const [answer, setAnswer] = useState(null);
  const [score, setScore] = useState(0);
  const svg = useRef(null);

  const rounds = game.rounds;
  const r = rounds[index];
  const finished = index >= rounds.length;
  const x = (v) => PAD + ((v - min) / (max - min)) * (W - 2 * PAD);
  const fromPointer = (e) => {
    const box = svg.current.getBoundingClientRect();
    const px = ((e.clientX - box.left) / box.width) * W;
    return snap(min + ((px - PAD) / (W - 2 * PAD)) * (max - min));
  };

  function check() {
    if (answer) return;
    const ok = Math.abs(value - r.value) <= tolerance + 1e-9;
    if (ok) { playCorrect(); setScore((s) => s + 1); } else playWrong();
    setAnswer({
      ok,
      text: `${ok ? '' : `You chose ${formatValue(value, format, step)}. `}${r.valueLabel ?? formatValue(r.value, format, step)} is here. ${r.explanation ?? ''}`.trim(),
      textHi: r.explanationHi
    });
  }

  function next() {
    setIndex((i) => i + 1);
    setValue(middle);
    setAnswer(null);
  }

  function key(e) {
    const moves = { ArrowRight: step, ArrowUp: step, ArrowLeft: -step, ArrowDown: -step, PageUp: 5 * step, PageDown: -5 * step };
    if (answer && e.key !== 'Enter') return;
    if (e.key in moves) { e.preventDefault(); setValue((v) => snap(v + moves[e.key])); }
    else if (e.key === 'Home') { e.preventDefault(); setValue(min); }
    else if (e.key === 'End') { e.preventDefault(); setValue(max); }
    else if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); if (answer) next(); else check(); }
  }

  const ticks = [];
  for (let i = 0, v = min; v <= max + 1e-9; i++, v = min + i * step) ticks.push({ v, labelled: i % labelEvery === 0 });

  if (finished) {
    return (
      <GameFrame game={game} lang={lang} kind="Number line / संख्या रेखा" score={`You got ${score} of ${rounds.length}.`}>
        <p className="gm-done">{score === rounds.length ? '🎉 Perfect!' : 'Good try! Play again to improve.'}{' '}
          <button type="button" className="gk-link" onClick={() => { setIndex(0); setScore(0); setValue(middle); setAnswer(null); }}>Play again</button></p>
      </GameFrame>
    );
  }

  const label = formatValue(value, format, step);
  return (
    <GameFrame game={game} lang={lang} kind="Number line / संख्या रेखा" score={`Round ${index + 1} of ${rounds.length} · score ${score}`}>
      <p className="gm-prompt"><Bi en={r.prompt} hi={r.promptHi} lang={lang} /></p>
      <svg ref={svg} className="gm-line" viewBox={`0 0 ${W} 130`} role="slider" tabIndex={0}
        aria-label={plain(r.prompt, r.promptHi, lang)} aria-valuemin={min} aria-valuemax={max} aria-valuenow={value} aria-valuetext={label}
        onKeyDown={key}
        onPointerDown={(e) => { if (answer) return; e.currentTarget.setPointerCapture(e.pointerId); setValue(fromPointer(e)); }}
        onPointerMove={(e) => { if (!answer && e.currentTarget.hasPointerCapture(e.pointerId)) setValue(fromPointer(e)); }}>
        <line x1={PAD - 20} x2={W - PAD + 20} y1="70" y2="70" className="gm-axis" />
        {ticks.map((t) => (
          <g key={t.v}>
            <line x1={x(t.v)} x2={x(t.v)} y1={t.labelled ? 58 : 63} y2={t.labelled ? 82 : 77} className="gm-tick" />
            {t.labelled && <text x={x(t.v)} y="106" textAnchor="middle" className="gm-tick-label">{formatValue(t.v, format, step)}</text>}
          </g>
        ))}
        {answer && !answer.ok && (
          <g className="gm-true">
            <circle cx={x(r.value)} cy="70" r="11" />
            <text x={x(r.value)} y="38" textAnchor="middle">✓ {r.valueLabel ?? formatValue(r.value, format, step)}</text>
          </g>
        )}
        <g className={`gm-marker${answer ? (answer.ok ? ' ok' : ' bad') : ''}`}>
          <path d={`M ${x(value)} 52 l -12 -20 h 24 z`} />
          <text x={x(value)} y="22" textAnchor="middle">{label}</text>
        </g>
      </svg>
      <div className="gm-actions">
        <button type="button" onClick={() => setValue((v) => snap(v - step))} disabled={Boolean(answer)} aria-label="Move left one step">−</button>
        <output aria-live="polite">{label}</output>
        <button type="button" onClick={() => setValue((v) => snap(v + step))} disabled={Boolean(answer)} aria-label="Move right one step">+</button>
        {answer
          ? <button type="button" className="gk-btn" onClick={next}>{index + 1 < rounds.length ? 'Next →' : 'See score'}</button>
          : <button type="button" className="gk-btn" onClick={check}>Check</button>}
      </div>
      <Feedback result={answer} lang={lang} />
    </GameFrame>
  );
}
