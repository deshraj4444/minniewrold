import { useEffect, useRef, useState } from 'react';
import { playClick, playCorrect, playWrong } from '../../lib/sound.js';
import { Bi, Feedback, GameFrame, shuffle } from './common.jsx';

// The best score lives only in this browser (no login, nothing sent anywhere). Storage can be blocked, hence try/catch.
const readBest = (key) => { try { return Number(localStorage.getItem(key)) || 0; } catch { return 0; } };
const writeBest = (key, v) => { try { localStorage.setItem(key, String(v)); } catch { /* storage unavailable */ } };

const letters = ['A', 'B', 'C', 'D'];

/**
 * A timed multiple-choice round (60 seconds unless the game says otherwise). Each answer shows right or wrong with
 * the explanation; press Next (or wait a moment) for the next question. Ends when time is up or the questions run out.
 */
export default function SpeedQuiz({ game, lang, storageKey }) {
  const seconds = game.seconds ?? 60;
  const bestKey = `gk.best.${storageKey ?? game.id}`;
  const [phase, setPhase] = useState('intro');
  const [questions, setQuestions] = useState([]);
  const [index, setIndex] = useState(0);
  const [chosen, setChosen] = useState(null);
  const [score, setScore] = useState(0);
  const [left, setLeft] = useState(seconds);
  const [best, setBest] = useState(() => readBest(bestKey));
  const [newBest, setNewBest] = useState(false);
  const endsAt = useRef(0);
  const auto = useRef(0);
  const scoreRef = useRef(0);
  scoreRef.current = score;

  function finish() {
    clearTimeout(auto.current);
    setPhase('done');
    const final = scoreRef.current;
    if (final > readBest(bestKey)) { writeBest(bestKey, final); setBest(final); setNewBest(true); }
  }

  useEffect(() => {
    if (phase !== 'play') return undefined;
    const t = setInterval(() => {
      const s = Math.max(0, Math.ceil((endsAt.current - Date.now()) / 1000));
      setLeft(s);
      if (s === 0) { clearInterval(t); finish(); }
    }, 250);
    return () => clearInterval(t);
  }, [phase]);

  useEffect(() => () => clearTimeout(auto.current), []);

  function start() {
    playClick();
    setQuestions(shuffle(game.questions));
    setIndex(0);
    setChosen(null);
    setScore(0);
    setNewBest(false);
    setLeft(seconds);
    endsAt.current = Date.now() + seconds * 1000;
    setPhase('play');
  }

  function next() {
    clearTimeout(auto.current);
    setChosen(null);
    if (index + 1 >= questions.length) finish();
    else setIndex((i) => i + 1);
  }

  function choose(i) {
    if (chosen !== null) return;
    setChosen(i);
    if (i === questions[index].answer) { playCorrect(); setScore((s) => s + 1); } else playWrong();
    auto.current = setTimeout(next, 2200);
  }

  if (phase === 'intro' || phase === 'done') {
    return (
      <GameFrame game={game} lang={lang} kind={`Speed quiz · ${seconds} s / स्पीड क्विज़`}>
        {phase === 'done' && (
          <p className="gm-done" role="status">
            {newBest ? '🏆 New best score! ' : ''}You scored <b>{score}</b>{questions.length ? ` (${index + (chosen === null ? 0 : 1)} answered)` : ''}.
          </p>
        )}
        <p className="gm-best">Your best on this device: <b>{best}</b></p>
        <button type="button" className="gk-btn" onClick={start}>{phase === 'done' ? 'Play again' : `Start (${seconds} seconds)`}</button>
      </GameFrame>
    );
  }

  const q = questions[index];
  const result = chosen === null ? null : { ok: chosen === q.answer, text: q.explanation, textHi: q.explanationHi };
  return (
    <GameFrame game={game} lang={lang} kind={`Speed quiz · ${seconds} s / स्पीड क्विज़`}>
      <div className="gm-hud">
        <span role="timer" aria-label={`${left} seconds left`} className={left <= 10 ? 'low' : ''}>⏱ {left}s</span>
        <span>Score {score}</span>
        <span>Q {index + 1}/{questions.length}</span>
      </div>
      <p className="gm-prompt"><Bi en={q.text} hi={q.textHi} lang={lang} /></p>
      <div className="gm-options">
        {q.options.map((o, i) => {
          const state = chosen === null ? '' : i === q.answer ? ' ok' : i === chosen ? ' bad' : '';
          return (
            <button key={i} type="button" className={`gm-option${state}`} disabled={chosen !== null} onClick={() => choose(i)}>
              <span className="gm-letter" aria-hidden="true">{chosen !== null && i === q.answer ? '✓' : chosen === i ? '✗' : letters[i]}</span>
              <Bi en={o} hi={q.optionsHi?.[i]} lang={lang} />
            </button>
          );
        })}
      </div>
      <Feedback result={result} lang={lang} />
      {chosen !== null && <div className="gm-actions"><button type="button" className="gk-btn" onClick={next}>Next →</button></div>}
    </GameFrame>
  );
}
