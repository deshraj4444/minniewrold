import { useCallback, useRef, useState } from 'react';
import { useSound } from '../../lib/sound.js';

// Shared pieces of the learning games. Every game is data-driven: it gets one object from a chapter file
// (see MinnieWorld.Web/Data/Seed/cbse/schema) and the page's language: 'both' | 'en' | 'hi'.

/** Text in the chosen language. In 'both' mode the Hindi follows on its own line. Hindi is marked lang="hi". */
export function Bi({ en, hi, lang, className }) {
  if (!hi || lang === 'en') return <span className={className}>{en}</span>;
  if (lang === 'hi') return <span className={className} lang="hi">{hi}</span>;
  return <span className={className}>{en}<span className="gm-hi" lang="hi">{hi}</span></span>;
}

/** Plain text for aria-labels and alerts: the chosen language, falling back to English. */
export const plain = (en, hi, lang) => (lang === 'hi' && hi ? hi : en);

export function shuffle(list) {
  const a = [...list];
  for (let i = a.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [a[i], a[j]] = [a[j], a[i]];
  }
  return a;
}

/**
 * The feedback line after each answer: a tick or cross and a word as well as the colour (colour is never the only
 * signal), plus the explanation. It's a live region, so screen readers announce it.
 */
export function Feedback({ result, lang }) {
  return (
    <div className={`gm-feedback ${result?.ok === true ? 'ok' : result?.ok === false ? 'bad' : ''}`} role="status" aria-live="polite">
      {result && (
        <>
          <b>{result.ok ? '✓ Correct / सही' : '✗ Not quite / सही नहीं'}</b>
          {result.text && <p><Bi en={result.text} hi={result.textHi} lang={lang} /></p>}
        </>
      )}
    </div>
  );
}

export function SoundToggle() {
  const [on, setOn] = useSound();
  return (
    <button type="button" className="gm-sound" aria-pressed={on} onClick={() => setOn(!on)} title={on ? 'Sound on' : 'Sound off'}>
      <span aria-hidden="true">{on ? '🔊' : '🔇'}</span><span className="visually-hidden">{on ? 'Sound on (turn off)' : 'Sound off (turn on)'}</span>
    </button>
  );
}

/** The box around a game: title, how to play, sound toggle, the game, and a score line. */
export function GameFrame({ game, lang, kind, score, children }) {
  return (
    <section className="gm" aria-labelledby={`gm-${game.id}`}>
      <header className="gm-head">
        <div>
          <span className="gm-kind">{kind}</span>
          <h3 id={`gm-${game.id}`}><Bi en={game.title} hi={game.titleHi} lang={lang} /></h3>
        </div>
        <SoundToggle />
      </header>
      {game.instructions && <p className="gm-how"><Bi en={game.instructions} hi={game.instructionsHi} lang={lang} /></p>}
      {children}
      {score && <p className="gm-score" aria-live="polite">{score}</p>}
    </section>
  );
}

/**
 * Drag with a finger or a mouse (pointer events), as well as tap-to-select then tap-to-place, which also covers
 * keyboards and switch users. Drop targets carry data-drop="<id>". A drag that ends on no target does nothing.
 * Returns { ghost, startDrag, wasDrag }: render `ghost`, call startDrag in onPointerDown, and ignore the click
 * that follows a drag with `wasDrag()`.
 */
export function useDrag(onDrop) {
  const [ghost, setGhost] = useState(null);
  const dragged = useRef(false);
  const drop = useRef(onDrop);
  drop.current = onDrop;

  const startDrag = useCallback((e, id, label) => {
    if (e.pointerType === 'mouse' && e.button !== 0) return;
    const s = { x0: e.clientX, y0: e.clientY, moved: false };
    dragged.current = false;
    const move = (ev) => {
      if (!s.moved && Math.hypot(ev.clientX - s.x0, ev.clientY - s.y0) > 8) s.moved = true;
      if (s.moved) {
        ev.preventDefault();
        setGhost({ label, x: ev.clientX, y: ev.clientY });
        document.querySelectorAll('.gm-over').forEach((el) => el.classList.remove('gm-over'));
        document.elementFromPoint(ev.clientX, ev.clientY)?.closest('[data-drop]')?.classList.add('gm-over');
      }
    };
    const end = (ev) => {
      window.removeEventListener('pointermove', move);
      window.removeEventListener('pointerup', end);
      window.removeEventListener('pointercancel', end);
      document.querySelectorAll('.gm-over').forEach((el) => el.classList.remove('gm-over'));
      setGhost(null);
      if (!s.moved || ev.type === 'pointercancel') return;
      dragged.current = true;
      const target = document.elementFromPoint(ev.clientX, ev.clientY)?.closest('[data-drop]');
      if (target) drop.current(id, target.dataset.drop);
    };
    window.addEventListener('pointermove', move, { passive: false });
    window.addEventListener('pointerup', end);
    window.addEventListener('pointercancel', end);
  }, []);

  const wasDrag = useCallback(() => {
    const d = dragged.current;
    dragged.current = false;
    return d;
  }, []);

  const ghostEl = ghost && <div className="gm-ghost" style={{ left: ghost.x, top: ghost.y }} aria-hidden="true">{ghost.label}</div>;
  return { ghost: ghostEl, startDrag, wasDrag };
}
