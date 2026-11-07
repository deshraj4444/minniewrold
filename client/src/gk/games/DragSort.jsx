import { useMemo, useState } from 'react';
import { playCorrect, playWrong } from '../../lib/sound.js';
import { Bi, Feedback, GameFrame, plain, shuffle, useDrag } from './common.jsx';

/**
 * Sort items into 2–4 labelled buckets (e.g. rational / irrational). Drag an item onto a bucket, or tap an item and
 * then the bucket. A right answer stays in the bucket; a wrong one goes back with the explanation.
 */
export default function DragSort({ game, lang }) {
  const [round, setRound] = useState(0);
  const items = useMemo(() => shuffle(game.items.map((it, i) => ({ ...it, id: it.id ?? `i${i}` }))), [game.items, round]);
  const [placed, setPlaced] = useState({});
  const [missed, setMissed] = useState(new Set());
  const [selected, setSelected] = useState(null);
  const [result, setResult] = useState(null);

  function place(itemId, bucketId) {
    const item = items.find((i) => i.id === itemId);
    if (!item || placed[itemId]) return;
    const bucket = game.buckets.find((b) => b.id === bucketId);
    setSelected(null);
    if (item.bucket === bucketId) {
      playCorrect();
      setPlaced((p) => ({ ...p, [itemId]: bucketId }));
      setResult({ ok: true, text: item.explanation ?? `${item.text} → ${bucket.label}`, textHi: item.explanationHi });
    } else {
      playWrong();
      setMissed((m) => new Set(m).add(itemId));
      setResult({
        ok: false,
        text: `${item.text} is not “${bucket.label}”. ${item.explanation ?? ''}`.trim(),
        textHi: item.explanationHi ? `${item.textHi ?? item.text} “${bucket.labelHi ?? bucket.label}” नहीं है। ${item.explanationHi}` : undefined
      });
    }
  }

  const { ghost, startDrag, wasDrag } = useDrag(place);
  const left = items.filter((i) => !placed[i.id]);
  const done = left.length === 0;
  const firstTry = items.filter((i) => !missed.has(i.id)).length;

  function restart() {
    setRound((r) => r + 1);
    setPlaced({});
    setMissed(new Set());
    setSelected(null);
    setResult(null);
  }

  return (
    <GameFrame game={game} lang={lang} kind="Sort it / छाँटिए"
      score={done ? `All ${items.length} sorted · ${firstTry}/${items.length} right first time` : `${items.length - left.length}/${items.length} sorted`}>
      <div className="gm-pool" aria-label="Items to sort">
        {left.map((it) => (
          <button key={it.id} type="button" className={`gm-chip${selected === it.id ? ' selected' : ''}`} aria-pressed={selected === it.id}
            onPointerDown={(e) => startDrag(e, it.id, plain(it.text, it.textHi, lang))}
            onClick={() => { if (!wasDrag()) setSelected((s) => (s === it.id ? null : it.id)); }}>
            <Bi en={it.text} hi={it.textHi} lang={lang} />
          </button>
        ))}
        {done && <p className="gm-done">🎉 Well done! <button type="button" className="gk-link" onClick={restart}>Play again</button></p>}
      </div>

      <div className={`gm-buckets n${game.buckets.length}`}>
        {game.buckets.map((b) => (
          <div key={b.id} className="gm-bucket" data-drop={b.id}>
            <button type="button" className="gm-bucket-head" data-drop={b.id} disabled={!selected}
              aria-label={selected ? `Put the selected item in ${plain(b.label, b.labelHi, lang)}` : plain(b.label, b.labelHi, lang)}
              onClick={() => selected && place(selected, b.id)}>
              <Bi en={b.label} hi={b.labelHi} lang={lang} />
              {selected && <small aria-hidden="true">Tap to place here</small>}
            </button>
            <ul>
              {items.filter((i) => placed[i.id] === b.id).map((i) => <li key={i.id} className="gm-placed">✓ <Bi en={i.text} hi={i.textHi} lang={lang} /></li>)}
            </ul>
          </div>
        ))}
      </div>
      <Feedback result={result} lang={lang} />
      {ghost}
    </GameFrame>
  );
}
