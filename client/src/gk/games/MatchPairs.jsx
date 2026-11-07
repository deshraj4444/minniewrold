import { useMemo, useState } from 'react';
import { playCorrect, playWrong } from '../../lib/sound.js';
import { Bi, Feedback, GameFrame, plain, shuffle, useDrag } from './common.jsx';

/** Match each left item (a term) with its right item (a definition). Tap one on each side, or drag left onto right. */
export default function MatchPairs({ game, lang }) {
  const [round, setRound] = useState(0);
  const pairs = useMemo(() => game.pairs.map((p, i) => ({ ...p, id: p.id ?? `p${i}` })), [game.pairs]);
  const leftOrder = useMemo(() => shuffle(pairs), [pairs, round]);
  const rightOrder = useMemo(() => shuffle(pairs), [pairs, round]);
  const [matched, setMatched] = useState(new Set());
  const [left, setLeft] = useState(null);
  const [wrong, setWrong] = useState(0);
  const [result, setResult] = useState(null);

  function tryMatch(leftId, rightId) {
    if (matched.has(leftId) || matched.has(rightId)) return;
    const pair = pairs.find((p) => p.id === leftId);
    const other = pairs.find((p) => p.id === rightId);
    setLeft(null);
    if (leftId === rightId) {
      playCorrect();
      setMatched((m) => new Set(m).add(leftId));
      setResult({ ok: true, text: pair.explanation ?? `${pair.left} ↔ ${pair.right}`, textHi: pair.explanationHi });
    } else {
      playWrong();
      setWrong((w) => w + 1);
      setResult({ ok: false, text: `“${pair.left}” does not go with “${other.right}”. Try another.`, textHi: pair.leftHi && other.rightHi ? `“${pair.leftHi}” का जोड़ा “${other.rightHi}” नहीं है।` : undefined });
    }
  }

  const { ghost, startDrag, wasDrag } = useDrag(tryMatch);
  const done = matched.size === pairs.length;

  return (
    <GameFrame game={game} lang={lang} kind="Match / मिलान"
      score={done ? `All ${pairs.length} matched · ${wrong} wrong tr${wrong === 1 ? 'y' : 'ies'}` : `${matched.size}/${pairs.length} matched`}>
      <div className="gm-match">
        <ul aria-label={plain(game.leftLabel ?? 'Terms', game.leftLabelHi, lang)}>
          {leftOrder.map((p) => (
            <li key={p.id}>
              <button type="button" className={`gm-chip${left === p.id ? ' selected' : ''}${matched.has(p.id) ? ' matched' : ''}`}
                disabled={matched.has(p.id)} aria-pressed={left === p.id}
                onPointerDown={(e) => startDrag(e, p.id, plain(p.left, p.leftHi, lang))}
                onClick={() => { if (!wasDrag()) setLeft((s) => (s === p.id ? null : p.id)); }}>
                {matched.has(p.id) && <span aria-hidden="true">✓ </span>}<Bi en={p.left} hi={p.leftHi} lang={lang} />
              </button>
            </li>
          ))}
        </ul>
        <ul aria-label={plain(game.rightLabel ?? 'Meanings', game.rightLabelHi, lang)}>
          {rightOrder.map((p) => (
            <li key={p.id}>
              <button type="button" data-drop={p.id} className={`gm-chip gm-target${matched.has(p.id) ? ' matched' : ''}`}
                disabled={matched.has(p.id) || !left}
                aria-label={left ? `Match with: ${plain(p.right, p.rightHi, lang)}` : undefined}
                onClick={() => left && tryMatch(left, p.id)}>
                {matched.has(p.id) && <span aria-hidden="true">✓ </span>}<Bi en={p.right} hi={p.rightHi} lang={lang} />
              </button>
            </li>
          ))}
        </ul>
      </div>
      {done && <p className="gm-done">🎉 All matched! <button type="button" className="gk-link" onClick={() => { setRound((r) => r + 1); setMatched(new Set()); setWrong(0); setResult(null); }}>Play again</button></p>}
      <Feedback result={result} lang={lang} />
      {ghost}
    </GameFrame>
  );
}
