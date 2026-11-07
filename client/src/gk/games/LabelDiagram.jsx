import { useMemo, useState } from 'react';
import { playCorrect, playWrong } from '../../lib/sound.js';
import { Bi, Feedback, GameFrame, plain, shuffle, useDrag } from './common.jsx';

/**
 * Put labels on the numbered spots of a picture. `image` is a site path, an https URL, or inline SVG markup (shown as
 * an <img>, so it can't run scripts). Spot positions are percentages of the picture's width (x) and height (y).
 */
export default function LabelDiagram({ game, lang }) {
  const [round, setRound] = useState(0);
  const spots = useMemo(() => game.hotspots.map((h, i) => ({ ...h, id: h.id ?? `h${i}`, n: i + 1 })), [game.hotspots]);
  const labels = useMemo(() => shuffle(spots), [spots, round]);
  const [done, setDone] = useState(new Set());
  const [selected, setSelected] = useState(null);
  const [result, setResult] = useState(null);

  const src = game.image.trim().startsWith('<svg')
    ? `data:image/svg+xml;charset=utf-8,${encodeURIComponent(game.image)}`
    : /^(\/|https:\/\/)/.test(game.image) ? game.image : '';

  function put(labelId, spotId) {
    if (done.has(labelId)) return;
    const label = spots.find((s) => s.id === labelId);
    const spot = spots.find((s) => s.id === spotId);
    setSelected(null);
    if (labelId === spotId) {
      playCorrect();
      setDone((d) => new Set(d).add(labelId));
      setResult({ ok: true, text: label.explanation ?? `${label.label} is spot ${spot.n}.`, textHi: label.explanationHi });
    } else {
      playWrong();
      setResult({ ok: false, text: `Spot ${spot.n} is not “${label.label}”. Look again.`, textHi: label.labelHi ? `स्थान ${spot.n} “${label.labelHi}” नहीं है।` : undefined });
    }
  }

  const { ghost, startDrag, wasDrag } = useDrag(put);
  const finished = done.size === spots.length;

  return (
    <GameFrame game={game} lang={lang} kind="Label it / नामांकन"
      score={finished ? `🎉 All ${spots.length} labels placed!` : `${done.size}/${spots.length} labelled`}>
      <div className="gm-diagram">
        {src && <img src={src} alt={plain(game.alt ?? game.title, game.altHi ?? game.titleHi, lang)} />}
        {spots.map((s) => (
          <button key={s.id} type="button" data-drop={s.id} className={`gm-spot${done.has(s.id) ? ' ok' : ''}`}
            style={{ left: `${s.x}%`, top: `${s.y}%` }}
            disabled={done.has(s.id)}
            aria-label={done.has(s.id) ? `Spot ${s.n}: ${plain(s.label, s.labelHi, lang)}` : selected ? `Put the selected label on spot ${s.n}` : `Spot ${s.n}, not labelled yet`}
            onClick={() => selected && put(selected, s.id)}>
            {done.has(s.id) ? '✓' : s.n}
          </button>
        ))}
      </div>
      <ol className="gm-spot-list" aria-label="Spots">
        {spots.map((s) => <li key={s.id}>{done.has(s.id) ? <>✓ <Bi en={s.label} hi={s.labelHi} lang={lang} /></> : '—'}</li>)}
      </ol>
      <div className="gm-pool" aria-label="Labels">
        {labels.filter((l) => !done.has(l.id)).map((l) => (
          <button key={l.id} type="button" className={`gm-chip${selected === l.id ? ' selected' : ''}`} aria-pressed={selected === l.id}
            onPointerDown={(e) => startDrag(e, l.id, plain(l.label, l.labelHi, lang))}
            onClick={() => { if (!wasDrag()) setSelected((x) => (x === l.id ? null : l.id)); }}>
            <Bi en={l.label} hi={l.labelHi} lang={lang} />
          </button>
        ))}
        {finished && <p className="gm-done">🎉 Done! <button type="button" className="gk-link" onClick={() => { setRound((r) => r + 1); setDone(new Set()); setResult(null); }}>Play again</button></p>}
      </div>
      <Feedback result={result} lang={lang} />
      {ghost}
    </GameFrame>
  );
}
