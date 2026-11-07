import { useMemo, useState } from 'react';
import { playCorrect, playWrong } from '../../lib/sound.js';
import { Bi, GameFrame, plain, shuffle, useDrag } from './common.jsx';

/** A shuffled order that is not already the right one (so there's always something to do). */
function scrambled(ids) {
  if (ids.length < 2) return ids;
  let order = shuffle(ids);
  while (order.every((id, i) => id === ids[i])) order = shuffle(ids);
  return order;
}

/**
 * Put events or steps in the right order. Move a row with its ↑ ↓ buttons, or drag it onto another row's place,
 * then press Check: every row shows whether it's in the right place, with its explanation.
 */
export default function Timeline({ game, lang }) {
  const items = useMemo(() => game.items.map((it, i) => ({ ...it, id: it.id ?? `t${i}` })), [game.items]);
  const correct = items.map((i) => i.id);
  const [order, setOrder] = useState(() => scrambled(correct));
  const [checked, setChecked] = useState(false);
  const [tries, setTries] = useState(0);

  function move(id, to) {
    setChecked(false);
    setOrder((o) => {
      const from = o.indexOf(id);
      if (from < 0 || to < 0 || to >= o.length || from === to) return o;
      const next = [...o];
      next.splice(from, 1);
      next.splice(to, 0, id);
      return next;
    });
  }

  const { ghost, startDrag, wasDrag } = useDrag((id, target) => move(id, Number(target)));
  const right = order.filter((id, i) => id === correct[i]).length;
  const solved = checked && right === order.length;

  function check() {
    setChecked(true);
    setTries((t) => t + 1);
    if (order.every((id, i) => id === correct[i])) playCorrect(); else playWrong();
  }

  return (
    <GameFrame game={game} lang={lang} kind="Put in order / क्रम में लगाइए"
      score={checked ? (solved ? `🎉 All in order in ${tries} tr${tries === 1 ? 'y' : 'ies'}!` : `${right}/${order.length} in the right place. Fix the red ones and check again.`) : null}>
      <ol className="gm-timeline">
        {order.map((id, i) => {
          const it = items.find((x) => x.id === id);
          const ok = id === correct[i];
          const label = plain(it.text, it.textHi, lang);
          return (
            <li key={id} data-drop={i} className={checked ? (ok ? 'ok' : 'bad') : ''}>
              <span className="gm-pos" aria-hidden="true">{i + 1}</span>
              <span className="gm-drag-handle" aria-hidden="true" onPointerDown={(e) => startDrag(e, id, label)}>⠿</span>
              <div className="gm-row-text">
                <Bi en={it.text} hi={it.textHi} lang={lang} />
                {checked && (
                  <small className="gm-row-result">
                    {ok ? '✓ Right place' : `✗ Should be number ${correct.indexOf(id) + 1}`}
                    {it.explanation && <> · <Bi en={it.explanation} hi={it.explanationHi} lang={lang} /></>}
                  </small>
                )}
              </div>
              <span className="gm-row-buttons">
                <button type="button" onClick={() => { if (!wasDrag()) move(id, i - 1); }} disabled={i === 0} aria-label={`Move “${label}” up`}>↑</button>
                <button type="button" onClick={() => { if (!wasDrag()) move(id, i + 1); }} disabled={i === order.length - 1} aria-label={`Move “${label}” down`}>↓</button>
              </span>
            </li>
          );
        })}
      </ol>
      <div className="gm-actions">
        {solved
          ? <button type="button" className="gk-btn" onClick={() => { setOrder(scrambled(correct)); setChecked(false); setTries(0); }}>Play again</button>
          : <button type="button" className="gk-btn" onClick={check}>Check order</button>}
      </div>
      <div className="visually-hidden" role="status" aria-live="polite">{checked ? `${right} of ${order.length} in the right place` : ''}</div>
      {ghost}
    </GameFrame>
  );
}
